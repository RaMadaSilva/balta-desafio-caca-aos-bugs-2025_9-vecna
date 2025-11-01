using BugStore.Domain.Common;
using BugStore.Domain.DataTransferObject;

namespace BugStore.Domain.Contracts.IRepositories; 

public interface IReportsRepository
{
    Task<PaginatedList<RevenueByPeriodDto>> GetOrdersByPeriodAsync(RevenueByPeriodParameters parameters, CancellationToken cancellationToken);
    Task<PaginatedList<BestCustomerDto>> GetBestCustomerAsync(BestCustomerParameters parameters, CancellationToken cancellationToken = default);
}
