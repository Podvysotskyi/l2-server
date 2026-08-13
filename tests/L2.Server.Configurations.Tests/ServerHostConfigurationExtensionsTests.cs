using L2.Server.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace L2.Server.Configurations.Tests;

public sealed class ServerHostConfigurationExtensionsTests
{
    [Fact]
    public async Task AddServerHost_configures_filtered_http_request_logging()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(ServerHostConfigurationExtensionsTests).Assembly.FullName,
            EnvironmentName = "Testing"
        });
        builder.AddServerHost("l2-server-api");
        await using var app = builder.Build();

        var options = app.Services.GetRequiredService<IOptions<HttpLoggingOptions>>().Value;

        Assert.Equal(
            HttpLoggingFields.RequestProperties |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.Duration,
            options.LoggingFields);
        Assert.Single(app.Services.GetServices<IHttpLoggingInterceptor>());
    }
}
