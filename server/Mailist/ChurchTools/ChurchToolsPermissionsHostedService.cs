using ChurchTools;
using ChurchTools.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.ChurchTools;

public class ChurchToolsPermissionsHostedService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ChurchToolsPermissionsHostedService> logger;

    public ChurchToolsPermissionsHostedService(IServiceProvider serviceProvider, ILogger<ChurchToolsPermissionsHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope serviceScope = serviceProvider.CreateScope();
            IChurchToolsApi churchTools = serviceScope.ServiceProvider.GetRequiredService<IChurchToolsApi>();

            GlobalPermissions permissions = await churchTools.GetGlobalPermissions(cancellationToken);

            if (!permissions.ChurchCore.AdministerPersons)
                logger.LogWarning("ChurchTools user does not have permission 'churchcore:administer persons'. Mailist will not work.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking ChurchTools permissions.");
        }
    }
}
