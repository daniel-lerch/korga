using Korga.Server.ChurchTools.Api;
using Korga.Server.Extensions;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
		DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
		IChurchToolsApiService churchTools = serviceScope.ServiceProvider.GetRequiredService<IChurchToolsApiService>();

		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(stoppingToken);

		await Synchronize(database, personMasterdata.GroupTypes, database.GroupTypes, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, stoppingToken);

		await Synchronize(database, personMasterdata.Roles, database.GroupRoles, x => new(x.Id, x.GroupTypeId, x.Name), (x, y) => { y.GroupTypeId = x.GroupTypeId; y.Name = x.Name; }, stoppingToken);

		await Synchronize(database, personMasterdata.Statuses, database.Status, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, stoppingToken);
	}

	private async ValueTask Synchronize<TSrc, TDest>(DatabaseContext database, List<TSrc> apiResponses, DbSet<TDest> cachedSet, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<int>
		where TDest : class, IIdentifiable<int>
	{
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<TDest> cachedValues = await cachedSet.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		foreach ((TSrc? response, TDest? entity) in apiResponses.ContrastWith<TSrc, TDest, int>(cachedValues))
		{
			if (response == null)
				cachedSet.Remove(entity!);
			else if (entity == null)
				cachedSet.Add(convert(response));
			else
				update(response, entity);
		}

		await database.SaveChangesAsync(cancellationToken);
	}
}
