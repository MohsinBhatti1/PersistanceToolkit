using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using System.Text;

namespace PersistanceToolkit.Persistance
{
    internal class EntityStateProcessor
    {
        private readonly BaseContext _dbContext;

        internal EntityStateProcessor(BaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        internal void SetState(object entity)
        {
            var entry = _dbContext.Entry(entity);
            entry.State = entry.IsKeySet ? EntityState.Modified : EntityState.Added;

            ProcessNavigations(entry); // Recursive call for deeper levels
        }
        private void ProcessNavigations(EntityEntry entry)
        {
            foreach (var navigation in entry.Navigations)
            {
                if (_dbContext.IsNavigationIgnoredOnUpdate(entry.Entity.GetType(), navigation.Metadata.Name))
                {
                    continue; // Skip setting state for ignored navigation
                }

                if (navigation.CurrentValue is IEnumerable<object> collection) // Handle collections
                {
                    foreach (var child in collection)
                    {
                        SetState(child); // Recursively process each child
                    }
                }
                else if (navigation.CurrentValue != null) // Handle single entities
                {
                    SetState(navigation.CurrentValue);
                }
            }
        }
        internal void DetachedAllTrackedEntries()
        {
            _dbContext.ChangeTracker.Clear();
        }
    }
}
