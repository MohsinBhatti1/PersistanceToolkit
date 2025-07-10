using Ardalis.Specification;
using PersistanceToolkit.Abstractions;

namespace PersistanceToolkit.Tests
{
    public class ParentTableSpec : BaseSpecification<ParentTable>
    {
        public ParentTableSpec()
        {
            Query.Include(c => c.ChildTables);
        }
    }
}
