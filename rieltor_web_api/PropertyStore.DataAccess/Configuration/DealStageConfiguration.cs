
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class DealStageConfiguration : IEntityTypeConfiguration<DealStageEntity>
    {
        public void Configure(EntityTypeBuilder<DealStageEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(s => s.Name)
                .HasMaxLength(DealStage.MAX_NAME_LENGTH)
                .IsRequired();

            builder.Property(s => s.Description)
                .HasMaxLength(DealStage.MAX_DESCRIPTION_LENGTH);

            builder.Property(s => s.Order).IsRequired();
            builder.Property(s => s.ExpectedDuration).IsRequired();
            builder.Property(s => s.CreatedAt).IsRequired();

            // Индексы
            builder.HasIndex(s => s.PipelineId);
            builder.HasIndex(s => s.Order);
            builder.HasIndex(s => new { s.PipelineId, s.Order }).IsUnique();

            // Связи
            builder.HasOne(s => s.Pipeline)
                .WithMany(p => p.Stages)
                .HasForeignKey(s => s.PipelineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Deals)
                .WithOne(d => d.CurrentStage)
                .HasForeignKey(d => d.CurrentStageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
