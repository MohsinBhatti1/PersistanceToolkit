using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Persistance;
using PersistanceToolkit.Repositories;
using PersistanceToolkit.Tests.Initializers;
using Xunit;

namespace PersistanceToolkit.Tests
{
    public class TestForTest : IClassFixture<PTKTestFixture>
    {
        private readonly PTKTestFixture _fixture;
        public TestForTest(PTKTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task TestToTest()
        {
            try
            {

                var user = new User { FirstName = "Mohsin", LastName = "Naeem" };
                await _fixture.UserRepository.Save(user);

                ParentTable parentTable = new ParentTable
                {
                    Title = "123",
                    ChildTables = new List<ChildTable> { new ChildTable() { ParentId = 1, Title = "456" }, new ChildTable() { ParentId = 1, Title = "789" } },
                    User = new User { FirstName = "Mohsin123", LastName = "Naeem" }
                };
                await _fixture.ParentTableRepository.Save(parentTable);

                var spec = new ParentTableSpec();
                var result = await _fixture.ParentTableRepository.FirstOrDefaultAsync(spec);
                result.ChildTables.First().Title = "124";
                await _fixture.ParentTableRepository.Save(result);
            }
            catch (Exception ex)
            {
                // Handle exception
                Assert.Fail(ex.Message);
            }
            Assert.Equal(1, 1);
        }
    }
}
