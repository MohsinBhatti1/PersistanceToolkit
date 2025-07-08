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
            var user = new User { FirstName = "Mohsin", LastName = "Naeem" };
            user.CreatedBy = 1;
            await _fixture.UserRepository.Save(user);

            ParentTable parentTable = new ParentTable
            {
                Id = 1,
                Title = "123",
                CreatedBy = 1,
                TenantId = 1,
                ChildTables = new List<ChildTable> { new ChildTable() { ParentId = 1, Title = "456" }, new ChildTable() { ParentId = 1, Title = "789" } },
                User = new User { Id = 1, FirstName = "Mohsin123", LastName = "Naeem" }
            };
            await _fixture.ParentTableRepository.Save(parentTable);

            var spec = new ParentTableSpec();
            var result = await _fixture.ParentTableRepository.ListAsync(spec);

            Assert.Equal(1, 1);
        }
    }
}
