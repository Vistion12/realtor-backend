using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class RequestConfiguration : IEntityTypeConfiguration<RequestEntity>
    {
        public void Configure(EntityTypeBuilder<RequestEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(r => r.Type).IsRequired();
            builder.Property(r => r.Status).IsRequired();
            builder.Property(r => r.Message).HasMaxLength(Request.MAX_MESSAGE_LENGTH).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();

            // Внешние ключи
            builder.HasOne(r => r.Client)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Индексы
            builder.HasIndex(r => r.ClientId);
            builder.HasIndex(r => r.PropertyId);
            builder.HasIndex(r => r.Type);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.CreatedAt);
        }
    }
}
