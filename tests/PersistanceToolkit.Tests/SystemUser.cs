using PersistanceToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Tests
{
    public class SystemUser : ISystemUser
    {
        public int UserId { get; set; }
        public int TenantId { get; set; }
    }

}
