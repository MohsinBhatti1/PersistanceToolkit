using Ardalis.Specification;
using PersistanceToolkit.Contract;
using System.Collections.Generic;
using System.Reflection;

namespace PersistanceToolkit.Abstractions.Specifications
{
    public abstract class BaseSpecification<T> : Specification<T>
        where T : BaseEntity
    {
        public bool IgnoreCompanyFilter { get; set; }
        public bool IncludeDeletedRecords { get; set; }
        protected BaseSpecification()
        {
            Query.PostProcessingAction(a =>
            {
                var lst = (a as IEnumerable<BaseEntity>)?.ToList() ?? new List<BaseEntity>();
                CaptureLoadTimeSnapshot(lst);
                return a;
            });
        }

        private void CaptureLoadTimeSnapshot(List<BaseEntity> lst)
        {
            foreach (var entity in lst)
            {
                Helper.TraverseEntities(entity, item =>
                {
                    item.CaptureLoadTimeSnapshot();
                });
            }
        }
    }
}