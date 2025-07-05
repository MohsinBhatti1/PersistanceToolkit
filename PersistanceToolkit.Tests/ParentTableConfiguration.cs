using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersistanceToolkit.Configuration;
using PersistanceToolkit.Persistance;

namespace PersistanceToolkit.Tests
{
    public class ParentTableConfiguration : BaseConfiguration<ParentTable>
    {
        public ParentTableConfiguration() : base()
        {
        }
        public override void Configure(EntityTypeBuilder<ParentTable> builder)
        {
            base.Configure(builder);

            builder.ToTable("ParentTable");

            builder.HasMany(p => p.ChildTables).WithOne().HasForeignKey(c => c.ParentId).IsRequired(false);

            builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.CreatedBy).IsRequired();

            builder.Ignore(p => p.IsDeleted);

            builder.IgnoreOnUpdate(pb => pb.User);
        }
    }
}
