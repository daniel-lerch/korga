using ChurchTools.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchTools;

public interface IChurchToolsApi : IDisposable
{
	Login? User { get; }
	ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken = default);
	ValueTask<List<Group>> GetGroups(IEnumerable<int> groupStatuses, CancellationToken cancellationToken = default);
	ValueTask<List<GroupsMember>> GetGroupMembers(CancellationToken cancellationToken = default);
    ValueTask<List<GroupMember>> GetGroupMembers(int groupId, CancellationToken cancellationToken = default);
	ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken = default);
	ValueTask<Person> GetPerson(CancellationToken cancellationToken = default);
	ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken = default);
	ValueTask<string> GetPersonLoginToken(int personId, CancellationToken cancellationToken = default);
	ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken = default);
    ValueTask<List<Service>> GetServices(CancellationToken cancellationToken = default);
    ValueTask<Service> GetService(int serviceId, CancellationToken cancellationToken = default);
    ValueTask<List<ServiceGroup>> GetServiceGroups(CancellationToken cancellationToken = default);
    ValueTask<List<Event>> GetEvents(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    ValueTask<GlobalPermissions> GetGlobalPermissions(CancellationToken cancellationToken = default);
    ValueTask<CustomModule> GetCustomModule(string key, CancellationToken cancellationToken = default);
    ValueTask<List<CustomModuleDataCategory>> GetCustomDataCategories(int moduleId, CancellationToken cancellationToken = default);
}
