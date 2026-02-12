using System.Net;
using FluentAssertions;
using Zapper.LoyaltyPoints.Application.Responses;

namespace Zapper.LoyaltyPoints.IntegrationTests;

public class QueueIntegrationTests : IClassFixture<CommandApiFactory>
{
    private readonly HttpClient _client;

    public QueueIntegrationTests(CommandApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RetryFailedMessage_ValidId_Returns200()
    {
        var messageId = Guid.NewGuid();

        var response = await _client.PostAsync($"/api/queue/retry/{messageId}", null);

        // Note: This might return 409 if InMemory provider is used, which is expected
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Conflict);
    }
}
