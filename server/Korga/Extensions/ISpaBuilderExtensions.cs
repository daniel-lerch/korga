using Korga.Configuration;
using Korga.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Korga.Extensions;

public static class ISpaBuilderExtensions
{
    public static ISpaBuilder UseVueSpaFileProvider(this ISpaBuilder builder)
    {
        var env = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var options = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<IOptionsMonitor<HostingOptions>>();
        var logger = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ILogger<VueSpaFileProvider>>();

        builder.Options.DefaultPage = "/index.html";
        builder.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            FileProvider = new VueSpaFileProvider(env.WebRootPath, options, logger)
        };
        builder.Options.StartupTimeout = default;

        return builder;
    }
}
