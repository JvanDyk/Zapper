namespace Zapper.LoyaltyPoints.Application.Queries;

using Zapper.LoyaltyPoints.Domain.Exceptions;

public sealed class GetMerchantByCodeQueryHandler(
    IMerchantRepository merchantRepository) : IRequestHandler<GetMerchantByCodeQuery, MerchantResponse>
{
    public async Task<MerchantResponse> Handle(GetMerchantByCodeQuery request, CancellationToken cancellationToken)
    {
        var merchant = await merchantRepository.GetByCodeAsync(request.MerchantCode, cancellationToken);
        if (merchant is null)
            throw new EntityNotFoundException("Merchant", request.MerchantCode);

        var response = new MerchantResponse
        {
            Id = merchant.Id,
            MerchantCode = merchant.MerchantCode,
            Name = merchant.Name,
            ContactEmail = merchant.ContactEmail,
            IsActive = merchant.IsActive,
            CreatedAt = merchant.CreatedAt
        };

        return response;
    }
}
