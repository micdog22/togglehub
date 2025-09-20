
using Microsoft.OpenApi.Models;
using ToggleHub.Core;
using ToggleHub.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToggleHub API",
        Version = "v1",
        Description = "Serviço de Feature Flags simples e extensível"
    });
});

builder.Services.AddControllers();

var dataPath = builder.Configuration["Data:Path"] ?? Path.Combine(AppContext.BaseDirectory, "data", "flags.json");
builder.Services.AddSingleton<IFlagStore>(_ => new FileFlagStore(dataPath));

builder.Services.AddSingleton<ApiKeyProvider>(_ => new ApiKeyProvider(builder.Configuration["TOGGLEHUB_API_KEY"] ?? Environment.GetEnvironmentVariable("TOGGLEHUB_API_KEY")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

public partial class Program { }
