using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Persistence;
using PersistanceToolkit.Repositories;
using PersistanceToolKit.Persistence.Persistance;
using System;

namespace PersistanceToolkit.Tests.Initializers
{
    internal static class DependencyInjectionSetup
    {
        internal static ServiceProvider InitializeServiceProvider()
        {
            var services = new ServiceCollection();

            RegisterDependncies(services);
            return services.BuildServiceProvider();
        }

        private static void RegisterDependncies(ServiceCollection services)
        {
            services.AddScoped<ISystemUser>(serviceProvider =>
            {
                return new SystemUser { TenantId = 1, UserId = 1 };
            });
            services.AddScoped<BaseContext>(serviceProvider =>
            {
                return CreateInMemoryContext();
            });

            services.AddPersistanceToolkit();
        }

        private static SystemContext CreateInMemoryContext()
        {
            var dbName = $"TestDB_{Guid.NewGuid()}";
            var options = new DbContextOptionsBuilder<BaseContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SystemContext(options);
        }
        private static SystemContext CreateDBContext()
        {
            return new SystemContext("Data Source=MOHSIN-PC\\SQLEXPRESS;Initial Catalog=TEST;Integrated Security=True;TrustServerCertificate=True;");
        }
    }
}
