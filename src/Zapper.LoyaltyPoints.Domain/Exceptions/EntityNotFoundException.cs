namespace Zapper.LoyaltyPoints.Domain.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, string key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
