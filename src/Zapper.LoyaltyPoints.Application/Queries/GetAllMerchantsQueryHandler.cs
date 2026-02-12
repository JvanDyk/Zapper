namespace Zapper.LoyaltyPoints.Application.Queries;

public sealed class GetAllMerchantsQueryHandler(
    IMerchantRepository merchantRepository) : IRequestHandler<GetAllMerchantsQuery, IReadOnlyList<MerchantResponse>>
{
    public async Task<IReadOnlyList<MerchantResponse>> Handle(GetAllMerchantsQuery request, CancellationToken cancellationToken)
    {
        var merchants = await merchantRepository.GetAllAsync(cancellationToken);

        var response = merchants.Select(m => new MerchantResponse
        {
            Id = m.Id,
            MerchantCode = m.MerchantCode,
            Name = m.Name,
            ContactEmail = m.ContactEmail,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        }).ToList();

        return response;
    }
}
