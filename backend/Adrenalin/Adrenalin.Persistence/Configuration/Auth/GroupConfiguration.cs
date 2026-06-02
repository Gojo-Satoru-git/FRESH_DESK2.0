using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adrenalin.Persistence.Configuration.Auth
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups", "auth");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
       .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(x => x.RegionCode)
            .HasColumnName("region_code");

        builder.Property(x => x.TierCode)
            .HasColumnName("tier_code");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(x => x.RowVersion)
       .HasColumnName("row_version")
       .IsRowVersion()
       .IsConcurrencyToken();
        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        // Audit Relationships
        builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.CreatedBy)
       .OnDelete(DeleteBehavior.SetNull);

builder.HasOne<User>()
       .WithMany()
       .HasForeignKey(x => x.UpdatedBy)
       .OnDelete(DeleteBehavior.SetNull);
      

        // Lookup Relationships

       

        // User Groups

        builder.HasMany(x => x.UserGroups)
            .WithOne(x => x.Group)
            .HasForeignKey(x => x.GroupId);
    }
}
}