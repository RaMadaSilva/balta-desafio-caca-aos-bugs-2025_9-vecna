
using BugStore.Application.Features.Reports.RevenueByPeriod;
using BugStore.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints.Reports; 

public static class RevenueByPeriodEndpoint
{
    public static void MapRevenueByPeriodEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/reports/revenue-by-period", HandleAsync)
            .WithTags("Reports")
            .WithName("GetRevenueByPeriod")
            .Produces<PagedResponse<RevenueByPeriodResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    static async Task<IResult> HandleAsync([AsParameters] RevenueByPeriodRequest request,
        [FromServices] ISender sender, 
        CancellationToken cancellationToken)
    {
         var result = await sender.Send(request, cancellationToken);
             return Results.Ok(result);
    }
}
