using BugStore.Application.Features.Reports.RevenueByPeriod;
using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;
using BugStore.Domain.DataTransferObject;
using FluentAssertions;
using Moq;

namespace BugStore.Application.Tests.Features.Reports;

public class RevenueByPeriodHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Aggregate_By_Year_And_Month_And_Sort()
    {
        // Total: 2*10 + 1*5 + 3*7 = 20 + 5 + 21 = 46
        var janRevenue = new RevenueByPeriodDto(2025, "January", 2, 46m);
        // Total: 1*100 = 100
        var febRevenue = new RevenueByPeriodDto(2025, "February", 1, 100m);

        var dto = new PaginatedList<RevenueByPeriodDto>(
            new List<RevenueByPeriodDto> { janRevenue, febRevenue },
            totalCount: 2,
            pageSize: 10,
            currentPage: 1);

        var reportsRepoMock = new Mock<IReportsRepository>();
        reportsRepoMock
            .Setup(r => r.GetOrdersByPeriodAsync(It.IsAny<RevenueByPeriodParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var handler = new RevenueByPeriodHandler(reportsRepoMock.Object);
        var request = new RevenueByPeriodRequest
        {
            StartPeriod = new DateTime(2025, 1, 1),
            EndPeriod = new DateTime(2025, 12, 31),
            PageNumber = 1,
            PageSize = 10
        };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);

        var jan = result.Items.First();
        jan.Year.Should().Be(2025);
        jan.Month.Should().Be("January");
        jan.TotalOrders.Should().Be(2);
        jan.TotalRevenue.Should().Be(46m);

        var feb = result.Items.Skip(1).First();
        feb.Year.Should().Be(2025);
        feb.Month.Should().Be("February");
        feb.TotalOrders.Should().Be(1);
        feb.TotalRevenue.Should().Be(100m);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Start_Greater_Than_End()
    {
        var reportsRepoMock = new Mock<IReportsRepository>();
        var handler = new RevenueByPeriodHandler(reportsRepoMock.Object);

        var request = new RevenueByPeriodRequest
        {
            StartPeriod = new DateTime(2025, 2, 1),
            EndPeriod = new DateTime(2025, 1, 1),
            PageNumber = 1,
            PageSize = 10
        };

        var act = async () => await handler.HandleAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}


