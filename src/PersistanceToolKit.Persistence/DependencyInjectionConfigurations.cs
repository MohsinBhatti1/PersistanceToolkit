using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Repositories;
using PersistanceToolKit.Persistence.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Persistence
{
    public static class PersistanceToolkitServiceCollectionExtensions
    {
        public static ServiceCollection AddPersistanceToolkit(this ServiceCollection services)
        {
            services.AddScoped(typeof(IGenericReadRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IEntityReadRepository<>), typeof(EntityRepository<>));
            services.AddScoped(typeof(IAggregateReadRepository<>), typeof(AggregateRepository<>));
            services.AddScoped(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
            return services;
        }
    }
}
