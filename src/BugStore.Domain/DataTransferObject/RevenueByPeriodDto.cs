namespace BugStore.Domain.DataTransferObject; 

public record RevenueByPeriodDto(
         int Year,
         string Month,
         long TotalOrders,
         decimal TotalRevenue); 
