using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Korga.Server.Database
{
    partial class DatabaseContext
    {
        public async Task<bool> UpdatePerson(Person person, Action<Person> update)
        {
            var (success, oldValues) = await ConcurrentUpdate(person, p => (p.GivenName, p.FamilyName, p.MailAddress), update);
            if (!success) return false;

            PersonSnapshots.Add(new PersonSnapshot(oldValues.GivenName, oldValues.FamilyName)
            {
                PersonId = person.Id,
                Version = person.Version,
                MailAddress = oldValues.MailAddress
            });

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMutableEntity<TEntity>(TEntity entity, int? deletedById) where TEntity : MutableEntityBase
        {
            while (true)
            {
                try
                {
                    // Do not delete a person multiple times as it would override DeletedById
                    if (entity.DeletionTime != default)
                        return false;

                    entity.DeletionTime = DateTime.UtcNow;
                    entity.DeletedById = deletedById;
                    entity.Version++;
                    await SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await UpdateTrackingEntity<TEntity>(ex);
                }
            }
        }

        private async Task<(bool success, TOldValues oldValues)> ConcurrentUpdate<TEntity, TOldValues>(TEntity entity, Func<TEntity, TOldValues> collector, Action<TEntity> update)
            where TEntity : MutableEntityBase
        {
            while (true)
            {
                try
                {
                    // Do not delete a person multiple times as it would override DeletedById
                    if (entity.DeletionTime != default)
                        return default;

                    TOldValues oldValues = collector(entity);
                    update(entity);
                    entity.Version++;
                    await SaveChangesAsync();
                    return (true, oldValues);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await UpdateTrackingEntity<TEntity>(ex);
                }
            }
        }

        private async Task UpdateTrackingEntity<TEntity>(DbUpdateConcurrencyException ex)
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
