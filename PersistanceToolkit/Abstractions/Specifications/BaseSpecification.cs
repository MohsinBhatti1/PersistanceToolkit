using Ardalis.Specification;
using PersistanceToolkit.Contract;

namespace PersistanceToolkit.Abstractions.Specifications
{
    public abstract class BaseSpecification<T> : Specification<T>
        where T : BaseEntity
    {
        public bool IgnoreCompanyFilter { get; set; }
        public bool IncludeDeletedRecords { get; set; }
        protected BaseSpecification()
        {
        }
    }
}