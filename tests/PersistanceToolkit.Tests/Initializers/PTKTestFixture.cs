using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Repositories;

namespace PersistanceToolkit.Tests.Initializers
{
    public class PTKTestFixture : IDisposable
    {
        public IAggregateRepository<ParentTable> ParentTableRepository;
        public IAggregateRepository<User> UserRepository;
        public SystemContext SystemContext;
        public PTKTestFixture()
        {
            var serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();

            SystemContext = serviceProvider.GetService<SystemContext>();
            var systemUser = serviceProvider.GetService<ISystemUser>();
            ParentTableRepository = serviceProvider.GetService<IAggregateRepository<ParentTable>>();
            UserRepository = serviceProvider.GetService<IAggregateRepository<User>>();
        }
        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
