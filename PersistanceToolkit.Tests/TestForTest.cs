using Microsoft.EntityFrameworkCore;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Persistance;
using PersistanceToolkit.Repositories;
using Xunit;

namespace PersistanceToolkit.Tests
{
    public class TestForTest
    {
        SystemContext dbContext;
        BaseRepository<ParentTable> repository;
        BaseRepository<User> userRepository;
        public TestForTest()
        {
            dbContext = CreateInMemoryContext();
            var systemUser = new SystemUser { CompanyId = 1, UserId = 1 };
            repository = new BaseRepository<ParentTable>(dbContext, systemUser);
            userRepository = new BaseRepository<User>(dbContext, systemUser);
        }
        private SystemContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BaseContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;
            return new SystemContext(options);
        }

        [Fact]
        public async Task TestToTest()
        {
            var user = new User { FirstName = "Mohsin", LastName = "Naeem" };
            user.CreatedBy = 1;
            await userRepository.Save(user);

            ParentTable parentTable = new ParentTable
            {
                Title = "123",
                CreatedBy = 1,
                TenantId = 1,
                ChildTables = new List<ChildTable> { new ChildTable(), new ChildTable() },
                User = new User { Id = 1, FirstName = "Mohsin123", LastName = "Naeem" }
            };
            await repository.Save(parentTable);

            var result1 = dbContext.ParentTables.AsNoTracking().ToList();

            var spec = new ParentTableSpec();
            var result = await repository.ListAsync(spec);

            Assert.Equal(1, 1);
        }
    }
}
