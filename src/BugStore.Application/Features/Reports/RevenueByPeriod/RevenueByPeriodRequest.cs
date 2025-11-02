using BugStore.Application.Shared;
using BugStore.Domain.Common;
using MediatR;

namespace BugStore.Application.Features.Reports.RevenueByPeriod; 

public class RevenueByPeriodRequest : CachedReportRequestBase, IRequest<PagedResponse<RevenueByPeriodResponse>>
{
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }

}
