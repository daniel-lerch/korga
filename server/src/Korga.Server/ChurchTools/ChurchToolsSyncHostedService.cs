using Korga.Server.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncHostedService : RepeatedExecutionService
{
	private readonly IServiceProvider serviceProvider;

	public ChurchToolsSyncHostedService(ILogger<ChurchToolsSyncHostedService> logger, IServiceProvider serviceProvider) : base(logger)
	{
		this.serviceProvider = serviceProvider;

		Interval = TimeSpan.FromSeconds(30);
	}

	protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
	{
		using IServiceScope serviceScope = serviceProvider.CreateScope();
		ChurchToolsSyncService syncService = serviceScope.ServiceProvider.GetRequiredService<ChurchToolsSyncService>();

		await syncService.Execute(stoppingToken);
	}
}
