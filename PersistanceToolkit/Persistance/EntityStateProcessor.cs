using Ardalis.Specification;
using PersistanceToolkit.Abstractions;
using PersistanceToolkit.Contract;
using System.Reflection;

namespace PersistanceToolkit.Persistance
{
    internal class EntityAuditLogSetter
    {
        private readonly ISystemUser _systemUser;

        public EntityAuditLogSetter(ISystemUser systemUser)
        {
            _systemUser = systemUser ?? throw new ArgumentNullException(nameof(systemUser));
        }

        internal void SetAuditLogsRecursively(BaseEntity rootEntity)
        {
            if (rootEntity == null)
                throw new ArgumentNullException(nameof(rootEntity));

            var now = DateTime.UtcNow;

            ProcessEntity(rootEntity, now);
        }

        internal void SetAuditLogsRecursively(IEnumerable<BaseEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetAuditLogsRecursively(entity);
            }
        }

        private void ProcessEntity(BaseEntity entity, DateTime timestamp)
        {
            entity.SetTenantId(_systemUser.TenantId);
            entity.SetAuditLogs(_systemUser.UserId, timestamp);

            var props = entity.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            foreach (var prop in props)
            {
                var value = prop.GetValue(entity);
                if (value == null) continue;

                switch (value)
                {
                    case BaseEntity childEntity:
                        ProcessEntity(childEntity, timestamp);
                        break;

                    case IEnumerable<BaseEntity> collection:
                        foreach (var item in collection)
                        {
                            if (item != null)
                                ProcessEntity(item, timestamp);
                        }
                        break;

                    case System.Collections.IEnumerable enumerable:
                        foreach (var item in enumerable)
                        {
                            if (item is BaseEntity nested)
                                ProcessEntity(nested, timestamp);
                        }
                        break;
                }
            }
        }
    }
}
