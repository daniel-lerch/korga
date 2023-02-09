using Korga.Server.ChurchTools.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public interface IChurchToolsApiService : IDisposable
{
	ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken);
	ValueTask<List<GroupMember>> GetGroupMembers(int groupId, CancellationToken cancellationToken);
	ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken);
	ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken);
	ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken);
}
