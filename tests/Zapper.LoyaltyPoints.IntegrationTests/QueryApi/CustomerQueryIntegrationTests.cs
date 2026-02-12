using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Infrastructure.Models;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class CustomerQueryIntegrationTests : IClassFixture<QueryApiFactory>
{
    private readonly HttpClient _client;

    public CustomerQueryIntegrationTests(QueryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllCustomers_Returns200()
    {
        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<CustomerResponse>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomerByCode_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/customers/cust-unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
