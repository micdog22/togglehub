
namespace ToggleHub.Core;

public record Flag
{
    public required string Key { get; init; }
    public string? Description { get; init; }
    public bool Enabled { get; init; } = true;
    public int RolloutPercent { get; init; } = 0; // 0..100
    public List<string> IncludeUserIds { get; init; } = new();
    public List<string> ExcludeUserIds { get; init; } = new();
    public Dictionary<string, List<string>> MatchAnyAttributes { get; init; } = new();
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public record EvaluateRequest
{
    public required string FlagKey { get; init; }
    public required string UserId { get; init; }
    public Dictionary<string, string>? Attributes { get; init; }
}

public record EvaluateResponse(string FlagKey, bool Enabled, string Reason);

public record UpsertFlagRequest
{
    public required string Key { get; init; }
    public string? Description { get; init; }
    public bool Enabled { get; init; } = true;
    public int RolloutPercent { get; init; } = 0;
    public List<string>? IncludeUserIds { get; init; }
    public List<string>? ExcludeUserIds { get; init; }
    public Dictionary<string, List<string>>? MatchAnyAttributes { get; init; }
}
