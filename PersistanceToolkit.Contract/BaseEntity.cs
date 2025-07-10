using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersistanceToolkit.Contract
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int TenantId { get; private set; }

        public int CreatedBy { get; private set; }
        public DateTime CreatedOn { get; private set; }

        public int UpdatedBy { get; private set; }
        public DateTime UpdatedOn { get; private set; }

        public bool IsDeleted { get; private set; }
        public int? DeletedBy { get; private set; }
        public DateTime? DeletedOn { get; private set; }

        [NotMapped]
        [JsonIgnore]
        private string LoadTimeSnapshot { get; set; } = string.Empty;


        public void MarkAsDeleted(int deletedBy, DateTime deletedOn)
        {
            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedOn = deletedOn;
        }
        public void SetAuditLogs(int userId, DateTime dateTime)
        {
            if (Id == 0)
            {
                CreatedBy = userId;
                CreatedOn = dateTime;
            }
            UpdatedBy = userId;
            UpdatedOn = dateTime;
        }

        public void SetTenantId(int tenantId)
        {
            TenantId = tenantId;
        }

        public bool HasChange()
        {
            return LoadTimeSnapshot != GetSnapshot();
        }
        public void CaptureLoadTimeSnapshot()
        {
            LoadTimeSnapshot = GetSnapshot();
        }
        private string GetSnapshot()
        {
            var actualType = GetType();
            return JsonSerializer.Serialize(
                this,
                actualType,
                new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
        }
    }
}
