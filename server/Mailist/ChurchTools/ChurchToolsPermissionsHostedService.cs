using ChurchTools;
using ChurchTools.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.ChurchTools
{
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

                await CheckSyncPermissions(permissions, churchTools, cancellationToken);
                CheckServiceHistoryPermissions(permissions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while checking ChurchTools permissions.");
            }
        }

        private async ValueTask CheckSyncPermissions(GlobalPermissions permissions, IChurchToolsApi churchTools, CancellationToken cancellationToken)
        {
            if (permissions.ChurchDb == null)
            {
                logger.LogWarning("ChurchTools people and groups module is disabled. Sync will not work.");
                return;
            }

            if (!permissions.ChurchCore.AdministerPersons)
                logger.LogWarning("ChurchTools user does not have permission 'churchcore:administer persons'. Sync will not work.");
            if (!permissions.ChurchDb.View)
                logger.LogWarning("ChurchTools user does not have permission 'churchdb:view'. Sync will not work.");
            if (permissions.ChurchDb.SecurityLevelPerson.Count == 0 || permissions.ChurchDb.SecurityLevelPerson.Max() < 3)
                logger.LogWarning("ChurchTools user does not have permission 'churchdb:security level person'. Sync will not work.");

            if (permissions.ChurchDb.View)
            {
                PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(cancellationToken);
                foreach (var department in personMasterdata.Departments)
                {
                    if (!permissions.ChurchDb.ViewAllData.Contains(department.Id))
                    {
                        logger.LogWarning("ChurchTools user does not have permission 'churchdb:view view alldata ({DepartmentId})'. Sync will not work.", department.Id);
                        break;
                    }
                }

                // API does not return churchdb:view group (-1) for super users although they can access all information.
                if (permissions.ChurchDb.ViewGroup.Count == 0)
                {
                    logger.LogWarning("ChurchTools user does not have permission 'churchdb:view group (-1)'. Sync will not work. " +
                        "This warning might also appear if the ChurchTools user is a super user.");
                }
            }
        }

        private void CheckServiceHistoryPermissions(GlobalPermissions permissions)
        {
            if (permissions.ChurchDb == null)
            {
                logger.LogWarning("ChurchTools people and groups module is disabled. Event history will not work.");
                return;
            }

            if (permissions.ChurchService == null)
            {
                logger.LogWarning("ChurchTools events module is disabled. Event history will not work.");
                return;
            }

            if (!permissions.ChurchDb.EditGroupMemberships)
                // https://forum.church.tools/topic/10368/berechtigungen-f%C3%BCr-groups-groupid-members-endpunkt
                logger.LogWarning("ChurchTools user does not have permission 'churchdb:edit group memberships'. Event history will not show comments.");
            if (!permissions.ChurchService.View)
                logger.LogWarning("ChurchTools user does not have permission 'churchservice:view'. Event history will not work.");
            if (permissions.ChurchService.ViewServiceGroup.Count == 0)
                logger.LogWarning("ChurchTools user does not have permission 'churchservice:view service group'. Event history will not work.");
            if (permissions.ChurchService.ViewEvents.Count == 0)
                logger.LogWarning("ChurchTools user does not have permission 'churchservice:view events'. Event history will not work.");
        }
    }
}
