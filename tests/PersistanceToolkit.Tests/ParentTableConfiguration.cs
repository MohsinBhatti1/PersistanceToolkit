using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersistanceToolKit.Persistence.Configuration;
using PersistanceToolKit.Persistence.Persistance;

namespace PersistanceToolkit.Tests
{
    public class ParentTableConfiguration : BaseConfiguration<ParentTable>
    {
        public override void Configure(EntityTypeBuilder<ParentTable> builder)
        {
            base.Configure(builder);

            builder.ToTable("ParentTable");

            builder.HasMany(p => p.ChildTables).WithOne().HasForeignKey(c => c.ParentId).IsRequired(false);

            builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.CreatedBy).IsRequired();

            //builder.Ignore(p => p.CreatedBy);
            //builder.Ignore(p => p.CreatedOn);
            //builder.Ignore(p => p.UpdatedBy);
            //builder.Ignore(p => p.UpdatedOn);
            //builder.Ignore(p => p.IsDeleted);
            //builder.Ignore(p => p.DeletedBy);
            //builder.Ignore(p => p.DeletedOn);

            builder.IgnoreOnUpdate(pb => pb.User);
            builder.IgnoreOnUpdate(pb => pb.IgnoredChild);
        }
    }
    public class ChildTableConfiguration : BaseConfiguration<ChildTable>
    {
        public override void Configure(EntityTypeBuilder<ChildTable> builder)
        {
            base.Configure(builder);

            builder.ToTable("ChildTable");

            builder.HasMany(c => c.GrandChildTables).WithOne().HasForeignKey(gc => gc.ChildId).IsRequired(false);

            //builder.Ignore(p => p.CreatedBy);
            //builder.Ignore(p => p.CreatedOn);
            //builder.Ignore(p => p.UpdatedBy);
            //builder.Ignore(p => p.UpdatedOn);
            //builder.Ignore(p => p.IsDeleted);
            //builder.Ignore(p => p.DeletedBy);
            //builder.Ignore(p => p.DeletedOn);
        }
    }
    
    public class GrandChildTableConfiguration : BaseConfiguration<GrandChildTable>
    {
        public override void Configure(EntityTypeBuilder<GrandChildTable> builder)
        {
            base.Configure(builder);

            builder.ToTable("GrandChildTable");

            //builder.Ignore(p => p.CreatedBy);
            //builder.Ignore(p => p.CreatedOn);
            //builder.Ignore(p => p.UpdatedBy);
            //builder.Ignore(p => p.UpdatedOn);
            //builder.Ignore(p => p.IsDeleted);
            //builder.Ignore(p => p.DeletedBy);
            //builder.Ignore(p => p.DeletedOn);
        }
    }
    public class UserConfiguration : BaseConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.ToTable("User");

            //builder.Ignore(p => p.CreatedBy);
            //builder.Ignore(p => p.CreatedOn);
            //builder.Ignore(p => p.UpdatedBy);
            //builder.Ignore(p => p.UpdatedOn);
            //builder.Ignore(p => p.IsDeleted);
            //builder.Ignore(p => p.DeletedBy);
            //builder.Ignore(p => p.DeletedOn);
        }
    }
}
