namespace Zapper.LoyaltyPoints.Domain.Exceptions;

public sealed class DuplicateTransactionException : Exception
{
    public string? TransactionId { get; }

    public DuplicateTransactionException(string transactionId)
        : base($"Purchase with transaction ID '{transactionId}' has already been processed.")
    {
        TransactionId = transactionId;
    }

    public DuplicateTransactionException(string transactionId, string message)
        : base(message)
    {
        TransactionId = transactionId;
    }
}
