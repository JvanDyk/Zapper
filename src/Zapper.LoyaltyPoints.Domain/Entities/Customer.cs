namespace Zapper.LoyaltyPoints.Domain.Entities;

public class Customer : BaseEntity
{
    public string CustomerCode { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    private Customer() { } // EF Core

    public static Customer Create(string customerCode, string name, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            CustomerCode = customerCode,
            Name = name,
            Email = email,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? email)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
