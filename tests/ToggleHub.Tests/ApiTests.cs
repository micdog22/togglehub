
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using ToggleHub.Core;
using Xunit;
using FluentAssertions;

namespace ToggleHub.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Create_And_Evaluate_Flag()
    {
        var client = _factory.CreateClient();
        var upsert = new UpsertFlagRequest
        {
            Key = "new-ui",
            Description = "Nova UI",
            Enabled = true,
            RolloutPercent = 50,
            IncludeUserIds = new() { "vip-1" },
            MatchAnyAttributes = new() { ["country"] = new() { "BR" } }
        };
        var create = await client.PostAsJsonAsync("/api/flags", upsert);
        create.EnsureSuccessStatusCode();

        var eval = await client.PostAsJsonAsync("/api/evaluate", new EvaluateRequest
        {
            FlagKey = "new-ui",
            UserId = "vip-1",
            Attributes = new() { ["country"] = "US" }
        });
        eval.EnsureSuccessStatusCode();
        var body = await eval.Content.ReadFromJsonAsync<EvaluateResponse>();
        body!.Enabled.Should().BeTrue();
        body!.Reason.Should().Be("included");
    }
}
