namespace Zapper.LoyaltyPoints.Domain.Exceptions;

public sealed class DomainValidationException : Exception
{
    public string Code { get; }
    public IDictionary<string, string[]> Errors { get; }

    public DomainValidationException(string code, string message)
        : base(message)
    {
        Code = code;
        Errors = new Dictionary<string, string[]>();
    }

    public DomainValidationException(string code, string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public DomainValidationException(string message)
        : base(message)
    {
        Code = "VALIDATION_ERROR";
        Errors = new Dictionary<string, string[]>();
    }

    public DomainValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Code = "VALIDATION_ERROR";
        Errors = errors;
    }
}
