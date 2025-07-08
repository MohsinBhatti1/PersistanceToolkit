using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersistanceToolkit.Contract
{
    public abstract class BaseEntity
    {
        [JsonIgnore]
        public string LoadTimeSnapshot { get; set; } = string.Empty;
        public bool HasChange()
        {
            return LoadTimeSnapshot != GetSnapshot();
        }
        public string GetSnapshot()
        {
            var actualType = GetType();
            return System.Text.Json.JsonSerializer.Serialize(
                this,
                actualType,
                new System.Text.Json.JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
        }
        public int Id { get; set; }
        public int TenantId { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }

        public void MarkAsDeleted(int deletedBy, DateTime deletedOn)
        {
            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedOn = deletedOn;
        }
        public void SetAuditLogs(int userId, DateTime dateTime)
        {
            if (CreatedBy == 0)
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
    }
}
