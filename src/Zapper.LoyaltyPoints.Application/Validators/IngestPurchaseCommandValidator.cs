using FluentValidation;
using Zapper.LoyaltyPoints.Application.Commands;

namespace Zapper.LoyaltyPoints.Application.Validators;

public sealed class IngestPurchaseCommandValidator : AbstractValidator<IngestPurchaseCommand>
{
    public IngestPurchaseCommandValidator()
    {
        RuleFor(x => x.PurchaseEvent).NotNull().WithMessage("Purchase event is required");

        RuleFor(x => x.PurchaseEvent.TransactionId)
            .NotEmpty().WithMessage("Transaction ID is required")
            .MaximumLength(256).WithMessage("Transaction ID must not exceed 256 characters")
            .When(x => x.PurchaseEvent is not null);

        RuleFor(x => x.PurchaseEvent.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required")
            .MaximumLength(256).WithMessage("Merchant ID must not exceed 256 characters")
            .When(x => x.PurchaseEvent is not null);

        RuleFor(x => x.PurchaseEvent.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required")
            .MaximumLength(256).WithMessage("Customer ID must not exceed 256 characters")
            .When(x => x.PurchaseEvent is not null);

        RuleFor(x => x.PurchaseEvent.Amount)
            .GreaterThan(0).WithMessage("Amount must be positive")
            .When(x => x.PurchaseEvent is not null);

        RuleFor(x => x.PurchaseEvent.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., ZAR)")
            .When(x => x.PurchaseEvent is not null);

        RuleFor(x => x.PurchaseEvent.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");

        RuleFor(x => x.PurchaseEvent.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .When(x => x.PurchaseEvent is not null);
    }
}
