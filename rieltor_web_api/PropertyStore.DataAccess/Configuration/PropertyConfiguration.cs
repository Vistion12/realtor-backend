using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class PropertyConfiguration : IEntityTypeConfiguration<PropertyEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(b => b.Title)
                .HasMaxLength(Property.MAX_TITLE_LENGTH)
                .IsRequired();

            builder.Property(b => b.Type)
                .IsRequired();

            builder.Property(b => b.Price)
                .IsRequired();

            builder.Property(b => b.Address)
                .IsRequired();

            builder.Property(b => b.Area)
                .IsRequired();

            builder.Property(b => b.Rooms)
                .IsRequired();

            builder.Property(b => b.Description)
                .IsRequired();

            //builder.Property(b => b.MainPhotoUrl).IsRequired();
            builder.HasMany(p => p.Images)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId);

            builder.Property(b => b.IsActive)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            // Индексы для часто запрашиваемых полей
            builder.HasIndex(b => b.Type);
            builder.HasIndex(b => b.Price);
            builder.HasIndex(b => b.IsActive);
            builder.HasIndex(b => b.CreatedAt);
        }
    }
}