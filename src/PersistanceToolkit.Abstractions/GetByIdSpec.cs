using Ardalis.Specification;
using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions
{
    public class GetByIdSpecification<T> : BaseSpecification<T>
        where T : Entity
    {
        public GetByIdSpecification(int id)
        {
            Query.Where(c => c.Id == id);
        }
    }
}