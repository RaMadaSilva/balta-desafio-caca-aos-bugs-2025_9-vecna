using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;
using BugStore.Domain.DataTransferObject;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BugStore.Infrastructure.Repositories
{
    public class ReportsRepository : BaseRepository<Order>, IReportsRepository
    {
        public ReportsRepository(AppDbContext context)
            : base(context) { }

        public async Task<PaginatedList<BestCustomerDto>> GetBestCustomerAsync(BestCustomerParameters parameters, CancellationToken cancellationToken = default)
        {
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var sql = @"
                        SELECT ""CustomerName"", ""CustomerEmail"", ""TotalOrders"", ""SpentAmount""
                        FROM (
                            SELECT 
                                c.""Name"" AS ""CustomerName"",
                                c.""Email"" AS ""CustomerEmail"",
                                CAST(COUNT(DISTINCT o.""Id"") AS INTEGER) AS ""TotalOrders"",
                                CAST(SUM(ol.""Quantity"" * p.""Price"") AS NUMERIC) AS ""SpentAmount""
                            FROM ""Orders"" o
                            INNER JOIN ""Customers"" c ON c.""Id"" = o.""CustomerId""
                            INNER JOIN ""OrderLines"" ol ON ol.""OrderId"" = o.""Id""
                            INNER JOIN ""Products"" p ON p.""Id"" = ol.""ProductId""
                            GROUP BY c.""Id"", c.""Name"", c.""Email""
                            ORDER BY ""SpentAmount"" DESC
                            LIMIT @Top
                        ) AS Ranked
                        LIMIT @PageSize OFFSET @Offset;

                        SELECT COUNT(*) 
                        FROM (
                            SELECT 1
                            FROM (
                                SELECT c.""Id"",
                                       CAST(SUM(ol.""Quantity"" * p.""Price"") AS NUMERIC) AS ""SpentAmount""
                                FROM ""Orders"" o
                                INNER JOIN ""Customers"" c ON c.""Id"" = o.""CustomerId""
                                INNER JOIN ""OrderLines"" ol ON ol.""OrderId"" = o.""Id""
                                INNER JOIN ""Products"" p ON p.""Id"" = ol.""ProductId""
                                GROUP BY c.""Id""
                                ORDER BY ""SpentAmount"" DESC
                                LIMIT @Top
                            ) AS T
                        ) AS CountTable;
                    ";

            var offset = (parameters.PageNumber - 1) * parameters.PageSize;

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                Top = parameters.Top,
                Offset = offset,
                PageSize = parameters.PageSize
            },
             commandTimeout: 300);

            var rawItems = (await multi.ReadAsync<BestCustomerDto>()).ToList();
            var items = rawItems.Select(r => new BestCustomerDto(
                r.CustomerName,
                r.CustomerEmail,
                (int)r.TotalOrders,
                (decimal)r.SpentAmount
            )).ToList();

            var totalCount = await multi.ReadSingleAsync<int>();

            return PaginatedList<BestCustomerDto>.ToPagedList(
                items, totalCount, parameters.PageNumber, parameters.PageSize);

        }

        public async Task<PaginatedList<RevenueByPeriodDto>> GetOrdersByPeriodAsync(RevenueByPeriodParameters parameters, CancellationToken cancellationToken)
        {
            // Obtem a conexão já configurada no DbContext (sem expor connection string)
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var offset = (parameters.PageNumber - 1) * parameters.PageSize;

            // SQLite: strftime retorna string → converter para INTEGER direto no SQL
            // Paginação movida para o SQL para melhor performance
            var sql = @"
                        SELECT 
                            EXTRACT(YEAR FROM o.""CreatedAt"")::INTEGER AS ""Year"",
                            TO_CHAR(o.""CreatedAt"", 'Month') AS ""Month"",
                            COUNT(o.""Id"") AS ""TotalOrders"",
                            CAST(SUM(ol.""Quantity"" * p.""Price"") AS DECIMAL(18,2)) AS ""TotalRevenue""
                        FROM public.""Orders"" o
                        INNER JOIN public.""OrderLines"" ol ON o.""Id"" = ol.""OrderId""
                        INNER JOIN public.""Products"" p ON ol.""ProductId"" = p.""Id""
                        WHERE o.""CreatedAt"" BETWEEN @StartDate AND @EndDate
                        GROUP BY ""Year"", ""Month""
                        ORDER BY ""Year"", ""Month""
                        LIMIT @PageSize OFFSET @Offset;

                        SELECT COUNT(DISTINCT TO_CHAR(o.""CreatedAt"", 'YYYY-MM')) as ""TotalCount""
                        FROM public.""Orders"" o
                        WHERE o.""CreatedAt"" BETWEEN @StartDate AND @EndDate;
                        ";


            // Executa a query com paginação no SQL usando QueryMultiple
            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                StartDate = parameters.StartPeriod,
                EndDate = parameters.EndPeriod,
                PageSize = parameters.PageSize,
                Offset = offset
            }, commandTimeout: 300);

            var rawItems = (await multi.ReadAsync<RevenueByPeriodDto>()).ToList();

            var items = rawItems.Select(r => new RevenueByPeriodDto(
                (int)r.Year,
                r.Month,
                r.TotalOrders,
                (decimal)r.TotalRevenue
            )).ToList();

            var totalCount = await multi.ReadSingleAsync<int>();

            return PaginatedList<RevenueByPeriodDto>.ToPagedList(
                items, totalCount, parameters.PageNumber, parameters.PageSize);
        }
    }
}
