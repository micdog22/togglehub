
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ToggleHub.Api.Auth;

public class ApiKeyProvider
{
    public string? Key { get; }
    public ApiKeyProvider(string? key) => Key = key;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var provider = context.HttpContext.RequestServices.GetService<ApiKeyProvider>();
        if (provider is null || string.IsNullOrWhiteSpace(provider.Key))
            return; // no key configured -> allow

        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var key) || key != provider.Key)
            context.Result = new UnauthorizedObjectResult(new { message = "Unauthorized" });
    }
}
