using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;
using BugStore.Domain.DataTransferObject;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace BugStore.Infrastructure.Repositories
{
    public class ReportsRepository : BaseRepository<Order>, IReportsRepository
    {
        public ReportsRepository(AppDbContext context)
            : base(context) { }

        //intemediario para pegar o total count junto com os dados paginados
        private class BestCustomerDtoInternal
        {
            public string CustomerName { get; set; }
            public string CustomerEmail { get; set; }
            public long TotalOrders { get; set; }
            public decimal SpentAmount { get; set; }
            public long TotalCount { get; set; }
        }; 

        public async Task<PaginatedList<BestCustomerDto>> GetBestCustomerAsync(BestCustomerParameters parameters, CancellationToken cancellationToken = default)
        {
        var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            var sql = @"
                        WITH CustomerSpending AS (
                            SELECT 
                                c.""Id"",
                                c.""Name"" AS ""CustomerName"",
                                c.""Email"" AS ""CustomerEmail"",
                                COUNT(DISTINCT o.""Id"") AS ""TotalOrders"",
                                SUM(ol.""Total"") AS ""SpentAmount""
                            FROM ""Orders"" o
                            JOIN ""Customers"" c ON c.""Id"" = o.""CustomerId""
                            JOIN ""OrderLines"" ol ON ol.""OrderId"" = o.""Id""
                            GROUP BY c.""Id"", c.""Name"", c.""Email""
                        ),
                        Ranked AS (
                            SELECT 
                                *,
                                COUNT(*) OVER() AS ""TotalCount""
                            FROM CustomerSpending
                            ORDER BY ""SpentAmount"" DESC
                            LIMIT @Top
                        )
                        SELECT ""CustomerName"", ""CustomerEmail"", ""TotalOrders"", ""SpentAmount"", ""TotalCount""
                        FROM Ranked
                        LIMIT @PageSize OFFSET @Offset;
                    ";

            var offset = (parameters.PageNumber - 1) * parameters.PageSize;

            var result = (await connection.QueryAsync<BestCustomerDtoInternal>(sql, new
            {
                Top = parameters.Top,
                Offset = offset,
                PageSize = parameters.PageSize
            }, commandTimeout: 60)).ToList();

            var totalCount = result.FirstOrDefault()?.TotalCount ?? 0;

            var items = result.Select(r => new BestCustomerDto(
                r.CustomerName,
                r.CustomerEmail,
                (int)r.TotalOrders,
                r.SpentAmount
            )).ToList();

            return PaginatedList<BestCustomerDto>.ToPagedList(
                items, (int)totalCount, parameters.PageNumber, parameters.PageSize);
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




