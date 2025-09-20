
using System.Security.Cryptography;
using System.Text;

namespace ToggleHub.Core;

public static class Engine
{
    public static EvaluateResponse Evaluate(Flag? flag, EvaluateRequest req)
    {
        if (flag is null)
            return new EvaluateResponse(req.FlagKey, false, "flag_not_found");

        if (!flag.Enabled)
            return new EvaluateResponse(flag.Key, false, "flag_disabled");

        if (flag.ExcludeUserIds.Contains(req.UserId))
            return new EvaluateResponse(flag.Key, false, "excluded");

        if (flag.IncludeUserIds.Contains(req.UserId))
            return new EvaluateResponse(flag.Key, true, "included");

        var attrs = req.Attributes ?? new Dictionary<string, string>();
        if (flag.MatchAnyAttributes.Count > 0)
        {
            foreach (var (attr, allowed) in flag.MatchAnyAttributes)
            {
                if (attrs.TryGetValue(attr, out var v) && allowed.Contains(v))
                    return new EvaluateResponse(flag.Key, true, "attributes_match");
            }
        }

        var bucket = PercentBucket(flag.Key, req.UserId);
        if (bucket < flag.RolloutPercent)
            return new EvaluateResponse(flag.Key, true, "rollout_on");

        return new EvaluateResponse(flag.Key, false, "rollout_off");
    }

    public static int PercentBucket(string flagKey, string userId)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{flagKey}:{userId}"));
        uint val = BitConverter.ToUInt32(bytes, 0);
        return (int)(val % 100);
    }
}
