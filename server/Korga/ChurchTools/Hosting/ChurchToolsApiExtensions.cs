using ChurchTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Korga.ChurchTools.Hosting;

public static class ChurchToolsApiExtensions
{
    public static IServiceCollection AddChurchToolsApi(this IServiceCollection services)
    {
        services.AddHttpClient("ChurchTools")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

        services.AddTransient<ChurchToolsApiFactory>();
        services.AddTransient<IChurchToolsApi>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ChurchToolsOptions>>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return ChurchToolsApi.CreateWithToken(httpClientFactory.CreateClient("ChurchTools"), options.Value.Host, options.Value.LoginToken);
        });

        return services;
    }
}
