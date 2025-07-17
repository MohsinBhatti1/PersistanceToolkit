using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Tests
{
    public class ParentTable : Entity, IAggregateRoot
    {
        public string Title { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ChildTable> ChildTables { get; set; }
        public virtual ChildTable IgnoredChild { get; set; }
    }
    public class ChildTable : Entity
    {
        public int ParentId { get; set; }
        public string Title { get; set; }
        public virtual ICollection<GrandChildTable> GrandChildTables { get; set; }
    }
    
    public class GrandChildTable : Entity
    {
        public int ChildId { get; set; }
        public string Title { get; set; }
    }
    public class User : Entity, IAggregateRoot
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}