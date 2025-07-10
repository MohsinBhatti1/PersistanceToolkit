using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Contract;
using PersistanceToolkit.Persistance;

namespace PersistanceToolkit.Repositories
{
    public class BaseRepository<T> : GenericRepository<T> where T : BaseEntity
    {
        private readonly ISystemUser _systemUser;
        public BaseRepository(BaseContext dbContext, ISystemUser systemUser) : base(dbContext)
        {
            _systemUser = systemUser;
        }

        #region Specification Methods
        protected override IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false)
        {
            if (specification is BaseSpecification<T>)
                SetBaseSpecification(specification as BaseSpecification<T>);

            return base.ApplySpecification(specification, evaluateCriteriaOnly);
        }
        protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        {
            if (specification is BaseSpecification<T>)
                SetBaseSpecification(specification as BaseSpecification<T>);

            return base.ApplySpecification(specification);
        }
        private void SetBaseSpecification(BaseSpecification<T> specification)
        {
            if
            (
                !(DbContext as BaseContext).IsPropertyIgnored<T>(c => c.TenantId) &&
                !specification.IgnoreCompanyFilter
            )
                specification.Query.Where(c => c.TenantId == _systemUser.TenantId);

            if
            (
                !(DbContext as BaseContext).IsPropertyIgnored<T>(c => c.IsDeleted) &&
                !specification.IncludeDeletedRecords
            )
                specification.Query.Where(c => !c.IsDeleted);
        }
        #endregion

        #region Save(Add & Update)/Delete Methods
        public override async Task<bool> Save(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            SetAuditLogs(entity);

            var result = await base.Save(entity, cancellationToken);

            if (result)
                ResetHasChanges(entity);

            return result;
        }

        private void ResetHasChanges(T entity)
        {
            Helper.TraverseEntities(entity, item =>
            {
                item.CaptureLoadTimeSnapshot();
            });
        }
        private void SetAuditLogs(T entity)
        {
            Helper.TraverseEntities(entity, item =>
            {
                if (item.HasChange())
                {
                    item.SetTenantId(_systemUser.TenantId);
                    item.SetAuditLogs(_systemUser.UserId, DateTime.Now);
                }
            });
        }
        private void SetAuditLogs(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                SetAuditLogs(entity);
            }
        }

        public override Task<bool> SaveRange(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            SetAuditLogs(entities);
            return base.SaveRange(entities, cancellationToken);
        }
        public override async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.MarkAsDeleted(_systemUser.UserId, DateTime.Now);

            DbContext.Set<T>().Attach(entity);
            DbContext.Entry(entity).Property(a => a.IsDeleted).IsModified = true;
            DbContext.Entry(entity).Property(a => a.DeletedOn).IsModified = true;
            DbContext.Entry(entity).Property(a => a.DeletedBy).IsModified = true;

            return await SaveChangesAsync(cancellationToken) > 0;
        }
        public override async Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.MarkAsDeleted(_systemUser.UserId, DateTime.Now);

                DbContext.Set<T>().Attach(entity);
                DbContext.Entry(entity).Property(a => a.IsDeleted).IsModified = true;
                DbContext.Entry(entity).Property(a => a.DeletedOn).IsModified = true;
                DbContext.Entry(entity).Property(a => a.DeletedBy).IsModified = true;
            }
            return await SaveChangesAsync(cancellationToken) > 0;
        }
        public override async Task<bool> DeleteRangeAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var entities = ApplySpecification(specification).ToList();
            foreach (var entity in entities)
            {
                entity.MarkAsDeleted(_systemUser.UserId, DateTime.Now);

                DbContext.Set<T>().Attach(entity);
                DbContext.Entry(entity).Property(a => a.IsDeleted).IsModified = true;
                DbContext.Entry(entity).Property(a => a.DeletedOn).IsModified = true;
                DbContext.Entry(entity).Property(a => a.DeletedBy).IsModified = true;
            }
            return await SaveChangesAsync(cancellationToken) > 0;
        }
        #endregion

    }
}
