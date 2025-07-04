using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using PersistanceToolkit.Persistance;
using System.Reflection;
using System.Text;

namespace PersistanceToolkit.Repositories
{
    public class GenericRepository<T> : RepositoryBase<T> where T : class
    {
        private readonly EntityStateProcessor _entityStateProcessor;
        public GenericRepository(BaseContext dbContext) : base(dbContext)
        {
            _entityStateProcessor = new EntityStateProcessor(dbContext);
        }

        public async Task<int> Save(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _entityStateProcessor.SetState(entity);
                return await SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Handle exception or log as needed
                return 0;
            }
        }
        public async Task<int> SaveRange(List<T> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var entity in entities)
                {
                    _entityStateProcessor.SetState(entity);
                }
                return await SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Handle exception or log as needed
                return 0;
            }
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
