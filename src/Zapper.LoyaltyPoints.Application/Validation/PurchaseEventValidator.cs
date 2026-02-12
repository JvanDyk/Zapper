using FluentValidation;

namespace Zapper.LoyaltyPoints.Application.Validation;

public sealed class PurchaseEventValidator : AbstractValidator<PurchaseEventResponse>
{
    public PurchaseEventValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required");

        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be positive");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., ZAR)");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required")
            .Must(BeInUtc).WithMessage("Timestamp must be in UTC");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required");
    }

    private static bool BeInUtc(DateTime dateTime) => dateTime.Kind == DateTimeKind.Utc;
}
