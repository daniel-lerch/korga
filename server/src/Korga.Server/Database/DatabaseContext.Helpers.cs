using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Korga.Server.Database
{
    partial class DatabaseContext
    {
        public async Task UpdatePerson(Person person, Action<Person> update)
        {
            var oldValues = await ConcurrentUpdate(person, p => (p.GivenName, p.FamilyName, p.MailAddress), update);

            PersonSnapshots.Add(new PersonSnapshot(oldValues.GivenName, oldValues.FamilyName)
            {
                PersonId = person.Id,
                Version = person.Version,
                MailAddress = oldValues.MailAddress
            });

            await SaveChangesAsync();
        }

        private async Task<TOldValues> ConcurrentUpdate<TEntity, TOldValues>(TEntity entity, Func<TEntity, TOldValues> collector, Action<TEntity> update) where TEntity : class
        {
            while (true)
            {
                try
                {
                    TOldValues oldValues = collector(entity);
                    update(entity);
                    await SaveChangesAsync();
                    return oldValues;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (ex.Entries.Count != 1 || !(ex.Entries[0].Entity is TEntity))
                        throw new NotSupportedException("Concurrency conflicts can only be handled for a single entity of type " + nameof(TEntity), ex);

                    var entry = ex.Entries[0];
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    entry.OriginalValues.SetValues(databaseValues);
                    entry.CurrentValues.SetValues(databaseValues);
                }
            }
        }
    }
}
