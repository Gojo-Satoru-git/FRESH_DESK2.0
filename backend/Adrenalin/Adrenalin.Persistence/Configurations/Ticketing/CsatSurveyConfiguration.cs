using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Adrenalin.Modules.Ticketing.Domain.Entities;

namespace Adrenalin.Persistence.Configurations.Ticketing;

public class CsatSurveyConfiguration : IEntityTypeConfiguration<CsatSurvey>
{
    public void Configure(EntityTypeBuilder<CsatSurvey> builder)
    {
        builder.HasKey(e => e.Id).HasName("csat_surveys_pkey");

        builder.ToTable("csat_surveys", "ticket");

        builder.HasIndex(e => e.ContactId, "idx_csat_surveys_contact");

        builder.HasIndex(e => e.Rating, "idx_csat_surveys_rating");

        builder.HasIndex(e => e.TicketId, "uq_csat_surveys_ticket").IsUnique();

        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()").HasColumnName("id");
        
        builder.Property(e => e.ContactId).HasColumnName("contact_id");
        
        builder.Property(e => e.Feedback).HasColumnName("feedback");
        
        builder.Property(e => e.Rating).HasColumnName("rating");
        
        builder.Property(e => e.SubmittedAt).HasDefaultValueSql("now()").HasColumnName("submitted_at");
        
        builder.Property(e => e.TicketId).HasColumnName("ticket_id");
        
        builder.Property(e => e.RowVersion).HasColumnName("row_version");

        builder.HasOne<Contact>().WithMany().HasForeignKey(d => d.ContactId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("csat_surveys_contact_id_fkey");

        builder.HasOne(d => d.Ticket).WithOne(p => p.CsatSurvey).HasForeignKey<CsatSurvey>(d => d.TicketId).HasConstraintName("csat_surveys_ticket_id_fkey");
    }
}
