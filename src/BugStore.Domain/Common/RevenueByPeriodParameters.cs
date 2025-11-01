namespace BugStore.Domain.Common; 

public class RevenueByPeriodParameters : RequestParameters
{
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }
}
