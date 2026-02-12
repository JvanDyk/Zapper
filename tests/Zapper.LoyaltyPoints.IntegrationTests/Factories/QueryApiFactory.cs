namespace Zapper.LoyaltyPoints.IntegrationTests;

// Note: The base CustomWebApplicationFactory is defined in CommandApiFactory.cs
// This file only defines the QueryApiFactory

public sealed class QueryApiFactory : CustomWebApplicationFactory<Zapper.LoyaltyPoints.Api.Queries.Program>;
