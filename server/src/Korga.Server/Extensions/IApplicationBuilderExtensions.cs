using Korga.Server.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Korga.Server.Extensions;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHosting(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<HostingOptions>>();

        if (options.Value.AllowProxies)
        {
            var headersOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All };
            headersOptions.KnownNetworks.Clear();
            headersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(headersOptions);
        }

        app.UsePathBase(options.Value.PathBase);

        return app;
    }

    public static IApplicationBuilder UseKorgaCors(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<HostingOptions>>();

        if (options.Value.CorsOrigins != null && options.Value.CorsOrigins.Length > 0)
        {
            app.UseCors(builder =>
            {
                builder
                    .WithOrigins(options.Value.CorsOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        }

        return app;
    }
}
