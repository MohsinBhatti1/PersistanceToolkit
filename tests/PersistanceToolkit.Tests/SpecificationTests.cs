using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Tests.Initializers;
using Xunit;

namespace PersistanceToolkit.Tests
{
    public class SpecificationTests : IClassFixture<PTKTestFixture>
    {
        private readonly PTKTestFixture _fixture;
        public SpecificationTests(PTKTestFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public async Task TestPagination()
        {
            try
            {
                var result = await _fixture.ParentTableRepository.SaveRange(GetParentTableObjects());

                var spec = new ParentTableSpec();
                var paginatedData = await _fixture.ParentTableRepository.PaginatedListAsync(spec, 980, 100);
                var allData = await _fixture.ParentTableRepository.ListAsync(spec);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Equal(1, 1);
        }
        private static List<ParentTable> GetParentTableObjects()
        {
            List<ParentTable> parentTables = new List<ParentTable>();
            for (int i = 0; i < 1000; i++)
            {
                parentTables.Add(new ParentTable
                {
                    Title = $"Parent {i}",
                    ChildTables = new List<ChildTable>
                    {
                        new ChildTable() { Title = $"Child {i}-1" },
                        new ChildTable() { Title = $"Child {i}-2" }
                    }
                });
            }
            return parentTables;
        }
    }
}
