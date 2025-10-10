
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class DealConfiguration : IEntityTypeConfiguration<DealEntity>
    {
        public void Configure(EntityTypeBuilder<DealEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(d => d.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(d => d.Notes)
                .HasMaxLength(Deal.MAX_NOTES_LENGTH);

            builder.Property(d => d.DealAmount)
                .HasPrecision(18, 2);

            builder.Property(d => d.ExpectedCloseDate);
            builder.Property(d => d.StageStartedAt).IsRequired();
            builder.Property(d => d.StageDeadline);
            builder.Property(d => d.CreatedAt).IsRequired();
            builder.Property(d => d.UpdatedAt);
            builder.Property(d => d.ClosedAt);
            builder.Property(d => d.IsActive).IsRequired();

            // Индексы
            builder.HasIndex(d => d.ClientId);
            builder.HasIndex(d => d.PipelineId);
            builder.HasIndex(d => d.CurrentStageId);
            builder.HasIndex(d => d.PropertyId);
            builder.HasIndex(d => d.RequestId);
            builder.HasIndex(d => d.IsActive);
            builder.HasIndex(d => d.CreatedAt);
            builder.HasIndex(d => d.StageDeadline);
            builder.HasIndex(d => d.ExpectedCloseDate);

            // Связи
            builder.HasOne(d => d.Pipeline)
                .WithMany(p => p.Deals)
                .HasForeignKey(d => d.PipelineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Client)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.CurrentStage)
                .WithMany(s => s.Deals)
                .HasForeignKey(d => d.CurrentStageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Property)
                .WithMany()
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(d => d.Request)
                .WithMany()
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(d => d.History)
                .WithOne(h => h.Deal)
                .HasForeignKey(h => h.DealId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
