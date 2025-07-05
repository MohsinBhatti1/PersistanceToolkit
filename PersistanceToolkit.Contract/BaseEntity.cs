namespace PersistanceToolkit.Contract
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}
