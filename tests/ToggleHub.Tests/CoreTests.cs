
using ToggleHub.Core;
using Xunit;
using FluentAssertions;

namespace ToggleHub.Tests;

public class CoreTests
{
    [Fact]
    public void Include_And_Exclude_Should_Take_Precedence()
    {
        var flag = new Flag
        {
            Key = "f",
            Enabled = true,
            IncludeUserIds = new() { "a" },
            ExcludeUserIds = new() { "b" },
            RolloutPercent = 0
        };

        Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "b" }).Enabled.Should().BeFalse();
        Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "a" }).Enabled.Should().BeTrue();
    }

    [Fact]
    public void Attributes_Should_Match()
    {
        var flag = new Flag
        {
            Key = "f",
            Enabled = true,
            MatchAnyAttributes = new() { ["country"] = new() { "BR", "PT" } }
        };

        Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "u1", Attributes = new() { ["country"] = "BR" } })
            .Enabled.Should().BeTrue();
        Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "u2", Attributes = new() { ["country"] = "US" } })
            .Enabled.Should().BeFalse();
    }

    [Fact]
    public void Rollout_Should_Be_Deterministic()
    {
        var flag = new Flag { Key = "f", Enabled = true, RolloutPercent = 100 };
        var r1 = Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "u1" });
        var r2 = Engine.Evaluate(flag, new EvaluateRequest { FlagKey = "f", UserId = "u1" });
        r1.Enabled.Should().BeTrue();
        r1.Enabled.Should().Be(r2.Enabled);
    }
}
