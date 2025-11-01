using BugStore.Application.Mappings.Reports;
using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;

namespace BugStore.Application.Features.Reports.BestCustomers; 

public class BestCustomerHandler
{
    private readonly IReportsRepository _repository;

    public BestCustomerHandler(IReportsRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<BestCustomersResponse>> HandleAsync(BestCustomersRequest request, CancellationToken cancellationToken)
    {
        var parameters = new BestCustomerParameters
        {
            Top = request.Top,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize

        };

        var bestCustomerDto = await _repository
                                        .GetBestCustomerAsync(parameters, cancellationToken);

        return ReportsMapping.ToPagedResponse(bestCustomerDto); 
    }

}
