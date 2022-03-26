using Microsoft.Extensions.Configuration;

namespace Korga.Server.Tests.Extensions;

public static class IConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddKorga(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", optional: false);
        builder.AddUserSecrets<Program>();

        return builder;
    }
}
