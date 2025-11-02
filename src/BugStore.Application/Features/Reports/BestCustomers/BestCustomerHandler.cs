using BugStore.Application.Mappings.Reports;
using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;
using MediatR;

namespace BugStore.Application.Features.Reports.BestCustomers; 

public class BestCustomerHandler : IRequestHandler<BestCustomersRequest, PagedResponse<BestCustomersResponse>>
{
    private readonly IReportsRepository _repository;

    public BestCustomerHandler(IReportsRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<BestCustomersResponse>> Handle(BestCustomersRequest request, CancellationToken cancellationToken)
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
