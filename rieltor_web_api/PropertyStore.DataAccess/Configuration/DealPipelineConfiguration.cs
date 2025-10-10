

using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class DealPipelineConfiguration : IEntityTypeConfiguration<DealPipelineEntity>
    {
        public void Configure(EntityTypeBuilder<DealPipelineEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(DealPipeline.MAX_NAME_LENGTH)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(DealPipeline.MAX_DESCRIPTION_LENGTH);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt);

            // Индексы
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.CreatedAt);

            // Связи
            builder.HasMany(p => p.Stages)
                .WithOne(s => s.Pipeline)
                .HasForeignKey(s => s.PipelineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Deals)
                .WithOne(d => d.Pipeline)
                .HasForeignKey(d => d.PipelineId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
