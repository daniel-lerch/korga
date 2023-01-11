using Korga.ChurchTools.Entities;
using Korga.Server.ChurchTools.Api;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncService
{
	private readonly IChurchToolsApiService churchTools;
	private readonly DatabaseContext database;

	public ChurchToolsSyncService(IChurchToolsApiService churchTools, DatabaseContext database)
	{
		this.churchTools = churchTools;
		this.database = database;
	}

	public async ValueTask Sync(CancellationToken cancellationToken)
	{
		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(cancellationToken);
		await SyncGroupTypes(personMasterdata.GroupTypes, cancellationToken);
	}

	private async ValueTask SyncGroupTypes(List<PersonMasterdata.GroupType> groupTypes, CancellationToken cancellationToken)
	{
		groupTypes.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<GroupType> dbGroupTypes = await database.GroupTypes.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new GroupTypeSynchronizer(database).Sync(groupTypes, dbGroupTypes);
		await database.SaveChangesAsync(cancellationToken);
	}

	private class GroupTypeSynchronizer : CollectionSynchronizer<PersonMasterdata.GroupType, GroupType, int>
	{
		private readonly DatabaseContext database;

		public GroupTypeSynchronizer(DatabaseContext database)
		{
			this.database = database;
		}

		protected override void Add(PersonMasterdata.GroupType src)
		{
			database.GroupTypes.Add(new GroupType(src.Id, src.Name));
		}

		protected override int GetDstKey(GroupType dest) => dest.Id;

		protected override int GetSrcKey(PersonMasterdata.GroupType src) => src.Id;

		protected override void Remove(GroupType dest)
		{
			database.GroupTypes.Remove(dest);
		}

		protected override void Update(PersonMasterdata.GroupType src, GroupType dest)
		{
			dest.Name = src.Name;
		}
	}
}
