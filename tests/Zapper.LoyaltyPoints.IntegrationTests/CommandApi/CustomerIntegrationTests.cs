using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Application.Commands;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class CustomerIntegrationTests : IClassFixture<CommandApiFactory>
{
    private readonly HttpClient _client;

    public CustomerIntegrationTests(CommandApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_ValidRequest_Returns201Created()
    {
        var customer = new CreateCustomerCommand(
            $"test-{Guid.NewGuid():N}",
            "Test Customer",
            $"test-{Guid.NewGuid():N}@example.com"
        );

        var response = await _client.PostAsJsonAsync("/api/customers", customer);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        body.Should().NotBeNull();
        body!.CustomerCode.Should().Be(customer.CustomerCode);
        body.Name.Should().Be(customer.Name);
        body.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task CreateCustomer_EmptyName_Returns400()
    {
        var customer = new CreateCustomerCommand(
            $"test-{Guid.NewGuid():N}",
            "",
            $"test-{Guid.NewGuid():N}@example.com"
        );

        var response = await _client.PostAsJsonAsync("/api/customers", customer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
