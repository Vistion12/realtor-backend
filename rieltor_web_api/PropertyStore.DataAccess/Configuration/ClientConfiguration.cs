using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class ClientConfiguration : IEntityTypeConfiguration<ClientEntity>
    {
        public void Configure(EntityTypeBuilder<ClientEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(c => c.Name).HasMaxLength(Client.MAX_NAME_LENGTH).IsRequired();
            builder.Property(c => c.Phone).HasMaxLength(Client.MAX_PHONE_LENGTH).IsRequired();
            builder.Property(c => c.Email).HasMaxLength(Client.MAX_EMAIL_LENGTH);
            builder.Property(c => c.Source).IsRequired();
            builder.Property(c => c.Notes).HasMaxLength(Client.MAX_NOTES_LENGTH);
            builder.Property(c => c.CreatedAt).IsRequired();

            // Индексы для поиска
            builder.HasIndex(c => c.Phone);
            builder.HasIndex(c => c.Email);
            builder.HasIndex(c => c.Source);
            builder.HasIndex(c => c.CreatedAt);
        }
    }
}
