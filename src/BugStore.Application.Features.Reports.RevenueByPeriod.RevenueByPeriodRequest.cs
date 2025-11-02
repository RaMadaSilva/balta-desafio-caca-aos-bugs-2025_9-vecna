using MediatR;

public class RevenueByPeriodRequest : CachedReportRequestBase, ICacheableRequest, IRequest<RevenueByPeriodResponse>
{
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }
}