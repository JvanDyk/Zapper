using Zapper.LoyaltyPoints.Application.Queries;
using Zapper.LoyaltyPoints.Domain.Exceptions;

namespace Zapper.LoyaltyPoints.Application.Services;

public sealed class PointsQueryService(
    ICustomerBalanceRepository balanceRepository,
    IPointsLedgerRepository ledgerRepository,
    IMerchantRepository merchantRepository) :
    IRequestHandler<GetBalancesQuery, CustomerBalanceSummaryResponse>,
    IRequestHandler<GetMerchantBalanceQuery, CustomerBalanceResponse>,
    IRequestHandler<GetPointsHistoryQuery, PointsHistoryResponse>
{
    public async Task<CustomerBalanceSummaryResponse> GetCustomerBalancesAsync(string customerId, CancellationToken ct = default)
    {
        // Fetch all balances for customer across all merchants
        var balances = await balanceRepository.GetAllByCustomerAsync(customerId, ct);

        if (balances is null || balances.Count == 0)
            throw new EntityNotFoundException("CustomerBalance", customerId);

        // Map to response DTOs
        return new CustomerBalanceSummaryResponse
        {
            CustomerId = customerId,
            Balances = [.. balances.Select(b => new CustomerBalanceResponse
            {
                CustomerId = b.CustomerId,
                MerchantId = b.MerchantId,
                TotalPoints = b.TotalPoints
            })]
        };
    }

    public async Task<CustomerBalanceResponse> GetCustomerMerchantBalanceAsync(
        string customerId, string merchantId, CancellationToken ct = default)
    {
        // Fetch balance for specific customer-merchant pair
        var balance = await balanceRepository.GetAsync(customerId, merchantId, ct);
        if (balance is null)
            throw new EntityNotFoundException("CustomerBalance", $"{customerId}/{merchantId}");

        // Map to response DTO
        return new CustomerBalanceResponse
        {
            CustomerId = balance.CustomerId,
            MerchantId = balance.MerchantId,
            TotalPoints = balance.TotalPoints
        };
    }

    public async Task<PointsHistoryResponse> GetPointsHistoryAsync(
        string customerId, string? merchantId = null, int limit = 20, int offset = 0, CancellationToken ct = default)
    {
        // Validate merchant exists if specified
        if (merchantId is not null)
        {
            var merchant = await merchantRepository.GetByCodeAsync(merchantId, ct);
            if (merchant is null)
                throw new EntityNotFoundException("Merchant", merchantId);
        }

        // Fetch ledger entries filtered by customer and optionally by merchant
        var entries = merchantId is not null
            ? await ledgerRepository.GetByCustomerAndMerchantAsync(customerId, merchantId, limit, offset, ct)
            : await ledgerRepository.GetByCustomerAsync(customerId, limit, offset, ct);

        if (entries is null || entries.Count == 0)
            throw new EntityNotFoundException("PointsHistory", customerId);

        // Map to response DTOs
        return new PointsHistoryResponse
        {
            CustomerId = customerId,
            Entries = [.. entries.Select(e => new PointsLedgerEntryResponse
            {
                Id = e.Id,
                TransactionId = e.TransactionId,
                MerchantId = e.MerchantId,
                Points = e.Points,
                PurchaseAmount = e.PurchaseAmount,
                Description = e.Description,
                CreatedAt = e.CreatedAt
            })]
        };
    }

    public async Task<CustomerBalanceSummaryResponse> Handle(GetBalancesQuery request, CancellationToken cancellationToken)
    {
        // Fetch all customer balances across merchants
        var balances = await balanceRepository.GetAllByCustomerAsync(request.CustomerId, cancellationToken);

        if (balances is null || balances.Count == 0)
            throw new EntityNotFoundException("CustomerBalance", request.CustomerId);

        // Map to response DTOs
        var response = new CustomerBalanceSummaryResponse
        {
            CustomerId = request.CustomerId,
            Balances = [.. balances.Select(b => new CustomerBalanceResponse
            {
                CustomerId = b.CustomerId,
                MerchantId = b.MerchantId,
                TotalPoints = b.TotalPoints
            })]
        };

        return response;
    }

    public async Task<CustomerBalanceResponse> Handle(GetMerchantBalanceQuery request, CancellationToken cancellationToken)
    {
        // Fetch balance for specific customer-merchant pair
        var balance = await balanceRepository.GetAsync(request.CustomerId, request.MerchantId, cancellationToken);
        if (balance is null)
            throw new EntityNotFoundException("CustomerBalance", $"{request.CustomerId}/{request.MerchantId}");

        // Map to response DTO
        var response = new CustomerBalanceResponse
        {
            CustomerId = balance.CustomerId,
            MerchantId = balance.MerchantId,
            TotalPoints = balance.TotalPoints
        };

        return response;
    }

    public async Task<PointsHistoryResponse> Handle(GetPointsHistoryQuery request, CancellationToken cancellationToken)
    {
        // Fetch ledger entries filtered by customer and optionally by merchant with pagination
        var entries = request.MerchantId is not null
            ? await ledgerRepository.GetByCustomerAndMerchantAsync(request.CustomerId, request.MerchantId, request.Limit, request.Offset, cancellationToken)
            : await ledgerRepository.GetByCustomerAsync(request.CustomerId, request.Limit, request.Offset, cancellationToken);

        if (entries is null || entries.Count == 0)
            throw new EntityNotFoundException("PointsHistory", request.CustomerId);

        // Map to response DTOs
        var response = new PointsHistoryResponse
        {
            CustomerId = request.CustomerId,
            Entries = entries.Select(e => new PointsLedgerEntryResponse
            {
                Id = e.Id,
                TransactionId = e.TransactionId,
                MerchantId = e.MerchantId,
                Points = e.Points,
                PurchaseAmount = e.PurchaseAmount,
                Description = e.Description,
                CreatedAt = e.CreatedAt
            }).ToList()
        };

        return response;
    }
}
