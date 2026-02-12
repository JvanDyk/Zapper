namespace Zapper.LoyaltyPoints.Application.Interfaces;

public interface IPointsQueryService
{
    Task<CustomerBalanceSummaryResponse> GetCustomerBalancesAsync(string customerId, CancellationToken ct = default);
    Task<CustomerBalanceResponse> GetCustomerMerchantBalanceAsync(string customerId, string merchantId, CancellationToken ct = default);
    Task<PointsHistoryResponse> GetPointsHistoryAsync(string customerId, string? merchantId = null, int limit = 20, int offset = 0, CancellationToken ct = default);
}
