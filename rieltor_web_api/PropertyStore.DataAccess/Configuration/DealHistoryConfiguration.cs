

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class DealHistoryConfiguration : IEntityTypeConfiguration<DealHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<DealHistoryEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(h => h.Notes);
            builder.Property(h => h.ChangedAt).IsRequired();
            builder.Property(h => h.TimeInStage).IsRequired();

            // Индексы
            builder.HasIndex(h => h.DealId);
            builder.HasIndex(h => h.FromStageId);
            builder.HasIndex(h => h.ToStageId);
            builder.HasIndex(h => h.ChangedAt);

            // Связи
            builder.HasOne(h => h.Deal)
                .WithMany(d => d.History)
                .HasForeignKey(h => h.DealId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.FromStage)
                .WithMany()
                .HasForeignKey(h => h.FromStageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.ToStage)
                .WithMany()
                .HasForeignKey(h => h.ToStageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
