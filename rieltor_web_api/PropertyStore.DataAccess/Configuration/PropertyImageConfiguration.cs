using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImageEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyImageEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(i => i.Url)
                .HasMaxLength(PropertyImage.MAX_URL_LENGTH)
                .IsRequired();

            builder.Property(i => i.IsMain)
                .IsRequired();

            builder.Property(i => i.Order)
                .IsRequired();

            // Внешний ключ
            builder.HasOne(i => i.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
