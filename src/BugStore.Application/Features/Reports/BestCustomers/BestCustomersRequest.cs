using BugStore.Application.Shared;
using BugStore.Domain.Common;
using MediatR;

namespace BugStore.Application.Features.Reports.BestCustomers; 

public class BestCustomersRequest: CachedReportRequestBase ,IRequest<PagedResponse<BestCustomersResponse>>
{
    public int Top { get; set; }

}
