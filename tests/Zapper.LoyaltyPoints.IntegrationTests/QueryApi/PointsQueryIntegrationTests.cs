using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Infrastructure.Models;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class PointsQueryIntegrationTests : IClassFixture<QueryApiFactory>
{
    private readonly HttpClient _client;

    public PointsQueryIntegrationTests(QueryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBalances_UnknownCustomer_Returns404()
    {
        var response = await _client.GetAsync("/api/customers/cust-unknown/points/balance");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task GetMerchantBalance_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/customers/cust-unknown/points/balance/merchant-unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task GetHistory_UnknownCustomer_Returns404()
    {
        var response = await _client.GetAsync("/api/customers/cust-unknown/points/history");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("NOT_FOUND");
    }
}
