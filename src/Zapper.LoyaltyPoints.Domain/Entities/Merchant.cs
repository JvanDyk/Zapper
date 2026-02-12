namespace Zapper.LoyaltyPoints.Domain.Entities;

public class Merchant : BaseEntity
{
    public string MerchantCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? ContactEmail { get; private set; }

    public bool IsActive { get; private set; }

    private Merchant() { } // EF Core

    public static Merchant Create(string merchantCode, string name, string? contactEmail = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(merchantCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Merchant
        {
            MerchantCode = merchantCode,
            Name = name,
            ContactEmail = contactEmail,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? contactEmail)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        ContactEmail = contactEmail;
        UpdatedAt = DateTime.UtcNow;
    }
}
