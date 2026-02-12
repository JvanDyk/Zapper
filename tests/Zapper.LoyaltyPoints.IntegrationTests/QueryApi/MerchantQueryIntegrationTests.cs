using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;
using Zapper.LoyaltyPoints.Infrastructure.Models;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class MerchantQueryIntegrationTests : IClassFixture<QueryApiFactory>
{
    private readonly HttpClient _client;

    public MerchantQueryIntegrationTests(QueryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllMerchants_Returns200()
    {
        var response = await _client.GetAsync("/api/merchants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<MerchantResponse>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMerchantByCode_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/merchants/merchant-unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("NOT_FOUND");
    }
}
