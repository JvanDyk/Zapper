namespace Zapper.LoyaltyPoints.Application.Commands;

public sealed record CreateCustomerCommand(string CustomerCode, string Name, string? Email) : IRequest<CustomerResponse>;
