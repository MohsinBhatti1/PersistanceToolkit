using Ardalis.Specification;
using PersistanceToolkit.Abstractions.Specifications;

namespace PersistanceToolkit.Tests
{
    public class ParentSpec : BaseSpecification<Parent>
    {
        public ParentSpec()
        {
            Query.Include(c => c.Children)
                 .ThenInclude(child => child.GrandChildren);
        }
    }
} 