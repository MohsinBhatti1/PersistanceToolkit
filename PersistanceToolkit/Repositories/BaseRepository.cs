using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Entities;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Persistance;
using System.Reflection;
using System.Text;

namespace PersistanceToolkit.Repositories
{
    public class BaseRepository<T> : GenericRepository<T>, IBaseRepository<T> where T : BaseEntity
    {
        private readonly ISystemUser _systemUser;
        public BaseRepository(BaseContext dbContext, ISystemUser systemUser) : base(dbContext)
        {
            _systemUser = systemUser;
        }
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
                !(DbContext as BaseContext).IsPropertyIgnored<T>("CompanyId") &&
                !specification.IgnoreCompanyFilter
            )
                specification.Query.Where(c => c.TenantId == _systemUser.CompanyId);

            if
            (
                !(DbContext as BaseContext).IsPropertyIgnored<T>("IsDeleted") &&
                !specification.IncludeDeletedRecords
            )
                specification.Query.Where(c => !c.IsDeleted);
        }
    }
}
