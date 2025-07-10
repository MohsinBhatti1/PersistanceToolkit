using Ardalis.Specification;
using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions
{
    public abstract class BaseSpecification<T> : Specification<T>
        where T : Entity
    {
        public bool IgnoreCompanyFilter { get; set; }
        public bool IncludeDeletedRecords { get; set; }
        protected BaseSpecification()
        {
            Query.PostProcessingAction(a =>
            {
                var lst = (a as IEnumerable<Entity>)?.ToList() ?? new List<Entity>();
                CaptureLoadTimeSnapshot(lst);
                return a;
            });
        }

        private void CaptureLoadTimeSnapshot(List<Entity> lst)
        {
            foreach (var entity in lst)
            {
                AggregateWalker.TraverseEntities(entity, item =>
                {
                    item.CaptureLoadTimeSnapshot();
                });
            }
        }
    }
}