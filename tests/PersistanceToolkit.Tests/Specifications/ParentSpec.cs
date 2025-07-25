using Ardalis.Specification;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Tests.Entities;

namespace PersistanceToolkit.Tests.Specifications
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