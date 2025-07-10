using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolKit.Persistence.Persistance;

namespace PersistanceToolkit.Repositories
{
    public class GenericRepository<T> : RepositoryBase<T>, IGenericRepository<T> where T : class
    {
        private readonly EntityStateProcessor _entityStateProcessor;
        public GenericRepository(BaseContext dbContext) : base(dbContext)
        {
            _entityStateProcessor = new EntityStateProcessor(dbContext);
        }

        public virtual async Task<bool> Save(T entity, CancellationToken cancellationToken = default)
        {
            _entityStateProcessor.SetState(entity);
            return await SaveChangesAsync(cancellationToken) > 0;
        }
        public virtual async Task<bool> SaveRange(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
                _entityStateProcessor.SetState(entity);

            return await SaveChangesAsync(cancellationToken) > 0;
        }
        public virtual new async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbContext.Set<T>().Remove(entity);
            return await SaveChangesAsync(cancellationToken) > 0;
        }

        /// <inheritdoc/>
        public virtual new async Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            DbContext.Set<T>().RemoveRange(entities);
            return await SaveChangesAsync(cancellationToken) > 0;
        }

        /// <inheritdoc/>
        public virtual new async Task<bool> DeleteRangeAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            DbContext.Set<T>().RemoveRange(query);

            return await SaveChangesAsync(cancellationToken) > 0;
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _entityStateProcessor.DetachedAllTrackedEntries();
            return result;
        }
        protected override IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false)
        {
            return base.ApplySpecification(specification, evaluateCriteriaOnly).AsNoTracking();
        }
    }
}
