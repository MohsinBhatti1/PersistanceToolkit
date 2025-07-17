using Ardalis.Specification;
using PersistanceToolkit.Abstractions.Specifications;

namespace PersistanceToolkit.Tests
{
    public class ParentTableSpec : BaseSpecification<ParentTable>
    {
        public ParentTableSpec()
        {
            Query.Include(c => c.ChildTables)
                 .ThenInclude(child => child.GrandChildTables);
        }
    }
}
