using BugStore.Application.Mappings.Orders;
using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;

namespace BugStore.Application.Features.Reports.RevenueByPeriod; 

public class RevenueByPeriodHandler
{
    private readonly IReportsRepository _repository;
    public RevenueByPeriodHandler(IReportsRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<RevenueByPeriodResponse>> HandleAsync(RevenueByPeriodRequest request, CancellationToken cancellationToken)
    {
         VerifyPeriod(request.StartPeriod, request.EndPeriod);

        var parameter = new RevenueByPeriodParameters
        {
            StartPeriod = request.StartPeriod,
            EndPeriod = request.EndPeriod, 
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var revenuesDtos = await _repository.GetOrdersByPeriodAsync(parameter, cancellationToken);

        return revenuesDtos.ToGetPagedResponse();
    }

    private void VerifyPeriod(DateTime startPeriod, DateTime endPeriod)
    {
        if(startPeriod > endPeriod)
        {
            throw new ArgumentException("StartPeriod não pode ser posterior a EndPeriod.");
        }
    }
}
