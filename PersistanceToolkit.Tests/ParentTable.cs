using PersistanceToolkit.Contract;

namespace PersistanceToolkit.Tests
{
    public class ParentTable : BaseEntity
    {
        public string Title { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ChildTable> ChildTables { get; set; }
    }
    public class ChildTable : BaseEntity
    {
        public int ParentId { get; set; }
        public int ChildTitle { get; set; }
    }
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}