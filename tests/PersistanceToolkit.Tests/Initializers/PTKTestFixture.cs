using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Repositories;

namespace PersistanceToolkit.Tests.Initializers
{
    public class PTKTestFixture : IDisposable
    {
        public IAggregateRepository<ParentTable> ParentTableRepository;
        public IEntityRepository<User> UserRepository;
        public PTKTestFixture()
        {
            var serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();

            var systemUser = serviceProvider.GetService<ISystemUser>();
            ParentTableRepository = serviceProvider.GetService<IAggregateRepository<ParentTable>>();
            UserRepository = serviceProvider.GetService<IEntityRepository<User>>();
        }
        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
