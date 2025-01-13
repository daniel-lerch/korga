using Korga.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.ChurchTools;

public class ChurchToolsSyncHostedService : RepeatedExecutionService
{
	private readonly IServiceProvider serviceProvider;

	public ChurchToolsSyncHostedService(IOptions<ChurchToolsOptions> options, ILogger<ChurchToolsSyncHostedService> logger, IServiceProvider serviceProvider) : base(logger)
	{
		this.serviceProvider = serviceProvider;

		Interval = TimeSpan.FromMinutes(options.Value.SyncIntervalInMinutes);
	}

	protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
	{
		using IServiceScope serviceScope = serviceProvider.CreateScope();
		ChurchToolsSyncService syncService = serviceScope.ServiceProvider.GetRequiredService<ChurchToolsSyncService>();

		await syncService.Execute(stoppingToken);
	}
}
