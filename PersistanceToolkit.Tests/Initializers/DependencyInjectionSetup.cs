using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Contract;
using PersistanceToolkit.Persistance;
using PersistanceToolkit.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Tests.Initializers
{
    internal static class DependencyInjectionSetup
    {
        internal static ServiceProvider InitializeServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddScoped<ISystemUser>(serviceProvider =>
            {
                return new SystemUser { TenantId = 1, UserId = 1 };
            });
            services.AddScoped<BaseContext>(serviceProvider =>
            {
                return CreateInMemoryContext();
            });
            services.AddScoped(typeof(BaseRepository<>), typeof(BaseRepository<>));
            return services.BuildServiceProvider();
        }
        private static SystemContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BaseContext>()
                .UseInMemoryDatabase("TestDB")
                .Options;
            return new SystemContext(options);
        }
        private static SystemContext CreateDBContext()
        {
            return new SystemContext("Data Source=10.4.0.8;Initial Catalog=iCM_Prod_Eagle;user id =mnaeem; password=Mnb@312419;TrustServerCertificate=True;");
        }
        private static void SeedDatabase(SystemContext context)
        {
            // Add your predefined data
            //context.Set<ScheduleTemplate>().Add(new MyEntity { Name = "Test Entity 1", IsActive = true });
            //context.Set<ScheduleTemplate>().Add(new MyEntity { Name = "Test Entity 2", IsActive = false });

            // Add more entities as needed
        }
    }
}
