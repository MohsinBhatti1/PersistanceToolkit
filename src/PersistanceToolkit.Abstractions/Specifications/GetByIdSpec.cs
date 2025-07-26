using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions.Specifications
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