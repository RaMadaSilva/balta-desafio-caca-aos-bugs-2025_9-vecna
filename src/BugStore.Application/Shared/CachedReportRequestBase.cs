using BugStore.Application.Contracts;
using BugStore.Domain.Common;

namespace BugStore.Application.Shared; 

public abstract class CachedReportRequestBase : RequestParameters, ICacheableRequest
{
    public virtual TimeSpan? CacheTtl => TimeSpan.FromMinutes(10);
    public virtual string CacheRegion => "reports";
    public virtual bool UseVersioning => true;
}
