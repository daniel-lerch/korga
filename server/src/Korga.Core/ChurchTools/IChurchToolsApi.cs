using Korga.ChurchTools.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.ChurchTools;

public interface IChurchToolsApi : IDisposable
{
	Login? User { get; }
	ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken = default);
	ValueTask<List<GroupMember>> GetGroupMembers(CancellationToken cancellationToken = default);
	ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken = default);
	ValueTask<Person> GetPerson(CancellationToken cancellationToken = default);
	ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken = default);
	ValueTask<string> GetPersonLoginToken(int personId, CancellationToken cancellationToken = default);
	ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken = default);
}
