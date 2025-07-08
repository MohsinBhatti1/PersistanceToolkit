using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Persistance;
using PersistanceToolkit.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PersistanceToolkit.Tests.Initializers
{
    public class PTKTestFixture : IDisposable
    {
        public BaseRepository<ParentTable> ParentTableRepository;
        public BaseRepository<User> UserRepository;
        public PTKTestFixture()
        {
            var serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();

            var systemUser = serviceProvider.GetService<ISystemUser>();
            ParentTableRepository = serviceProvider.GetService<BaseRepository<ParentTable>>();
            UserRepository = serviceProvider.GetService<BaseRepository<User>>();
        }
        public void Dispose()
        {
            // Cleanup resources
        }
    }
}
