using Ardalis.Specification;
using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions
{
    public abstract class BaseSpecification<T> : Specification<T>
        where T : Entity
    {
        public bool IgnoreCompanyFilter { get; set; }
        public bool IncludeDeletedRecords { get; set; }
        public BaseSpecification()
        {
            Query.PostProcessingAction(a =>
            {
                var lst = (a as IEnumerable<Entity>)?.ToList() ?? new List<Entity>();
                CaptureLoadTimeSnapshot(lst);
                RemoveDeletedEntries(lst);
                return a;
            });
        }

        private void RemoveDeletedEntries(List<Entity> lst)
        {
            if (IncludeDeletedRecords) return;
            foreach (var item in lst)
            {
                RemoveDeletedEntries(item);
            }
        }

        private void RemoveDeletedEntries(Entity entity)
        {
            AggregateWalker.TraverseEntities(entity, item =>
            {
                if (entity.IsDeleted)
                {
                    var props = entity.GetType()
                                            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                        .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(item);
                        if (value == null) continue;

                        switch (value)
                        {
                            case IEnumerable<Entity> collection:
                                if (value is IList<Entity> list)
                                {
                                    for (int i = list.Count - 1; i >= 0; i--)
                                    {
                                        if (list[i].IsDeleted)
                                        {
                                            list.RemoveAt(i);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
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
    public abstract class BaseSpecification<T, TResult> : Specification<T, TResult>
        where T : Entity
    {
        public bool IgnoreCompanyFilter { get; set; }
        public bool IncludeDeletedRecords { get; set; }
    }
}