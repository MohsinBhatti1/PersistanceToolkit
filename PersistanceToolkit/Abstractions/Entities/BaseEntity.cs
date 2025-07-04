namespace PersistanceToolkit.Abstractions.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; internal set; }
        public int TenantId { get; internal set; }

        public int CreatedBy { get; internal set; }
        public DateTime CreatedOn { get; internal set; }

        public int UpdatedBy { get; internal set; }
        public DateTime UpdatedOn { get; internal set; }

        public bool IsDeleted { get; internal set; }
        public int? DeletedBy { get; internal set; }
        public DateTime? DeletedOn { get; internal set; }
    }
}
