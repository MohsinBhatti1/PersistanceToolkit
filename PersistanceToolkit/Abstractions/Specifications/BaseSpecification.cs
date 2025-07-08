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
                SetJsonData(lst);
                return a;
            });
        }

        private void SetJsonData(List<BaseEntity> lst)
        {
            foreach (var item in lst)
            {
                SetJsonData(item);
            }
        }

        private void SetJsonData(BaseEntity entity)
        {
            SetJsonSnapShot(entity);

            var props = entity.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                var value = prop.GetValue(entity);
                if (value == null) continue;

                switch (value)
                {
                    case BaseEntity childEntity:
                        SetJsonData(childEntity);
                        break;

                    case IEnumerable<BaseEntity> collection:
                        foreach (var item in collection)
                        {
                            if (item != null)
                                SetJsonData(item);
                        }
                        break;

                    case System.Collections.IEnumerable enumerable:
                        foreach (var item in enumerable)
                        {
                            if (item is BaseEntity nested)
                                SetJsonData(nested);
                        }
                        break;
                }
            }
        }
        private void SetJsonSnapShot(BaseEntity entity)
        {
            entity.LoadTimeSnapshot = entity.GetSnapshot();

        }
    }
}