using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolKit.Persistence.Persistance;
using System.Data.SqlTypes;

namespace PersistanceToolkit.Repositories
{
    public class GenericRepository<T> : RepositoryBase<T>, IGenericRepository<T> where T : class
    {
        private const int Skip = 0;
        private const int Take = 20;
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
        public virtual new async Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            DbContext.Set<T>().RemoveRange(entities);
            return await SaveChangesAsync(cancellationToken) > 0;
        }
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
        public override async Task<T> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var queryResult = await base.FirstOrDefaultAsync(specification, cancellationToken);

            if (queryResult == null) return null;

            return ResultWithPostProcessingSpecificationAction(specification, queryResult);
        }
        public override async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : default
        {
            var queryResult = await base.FirstOrDefaultAsync(specification, cancellationToken);

            if (queryResult == null) return default;

            return ResultWithPostProcessingSpecificationAction(specification, queryResult);
        }
        public override async Task<T> SingleOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var queryResult = await base.SingleOrDefaultAsync(specification, cancellationToken);

            if (queryResult == null) return null;

            return ResultWithPostProcessingSpecificationAction(specification, queryResult);
        }
        public override async Task<TResult?> SingleOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : default
        {
            var queryResult = await base.SingleOrDefaultAsync(specification, cancellationToken);

            if (queryResult == null) return default;

            return ResultWithPostProcessingSpecificationAction(specification, queryResult);
        }
        private T ResultWithPostProcessingSpecificationAction(ISpecification<T> specification, T queryResult)
        {
            return specification.PostProcessingAction is null
                ? queryResult
                : specification.PostProcessingAction(new List<T> { queryResult }).SingleOrDefault();
        }
        private TResult ResultWithPostProcessingSpecificationAction<TResult>(ISpecification<T, TResult> specification, TResult queryResult)
        {
            return specification.PostProcessingAction is null
                ? queryResult
                : specification.PostProcessingAction(new List<TResult> { queryResult }).SingleOrDefault();
        }

        #region List Methods with pagination
        public async Task<List<T>> PaginatedListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            ImplementPagination(specification, Skip, 20);
            return await base.ListAsync(specification, cancellationToken);
        }
        public async Task<List<T>> PaginatedListAsync(ISpecification<T> specification, int skip, int take, CancellationToken cancellationToken = default)
        {
            ImplementPagination(specification, skip, take);
            return await base.ListAsync(specification, cancellationToken);
        }
        public async Task<List<TResult>> PaginatedListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            ImplementPagination(specification, 0, Take);
            return await base.ListAsync(specification, cancellationToken);
        }
        public async Task<List<TResult>> PaginatedListAsync<TResult>(ISpecification<T, TResult> specification, int skip, int take, CancellationToken cancellationToken = default)
        {
            ImplementPagination(specification, skip, take);
            return await base.ListAsync(specification, cancellationToken);
        }
        private static void ImplementPagination(ISpecification<T> specification, int skip, int take)
        {
            specification.Query.Skip(skip); // Reset skip for pagination
            specification.Query.Take(take); // Set a default page size, can be adjusted as needed
        }
        #endregion
    }
}
