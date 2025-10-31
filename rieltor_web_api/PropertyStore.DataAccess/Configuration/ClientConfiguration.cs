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

            // НОВЫЕ ПОЛЯ ДЛЯ ЛК
            builder.Property(c => c.HasPersonalAccount).HasDefaultValue(false);
            builder.Property(c => c.AccountLogin).HasMaxLength(Client.MAX_EMAIL_LENGTH);
            builder.Property(c => c.TemporaryPassword).HasMaxLength(Client.MAX_TEMP_PASSWORD_LENGTH);
            builder.Property(c => c.IsAccountActive).HasDefaultValue(false);
            builder.Property(c => c.ConsentToPersonalData).HasDefaultValue(false);
            builder.Property(c => c.ConsentGivenAt);
            builder.Property(c => c.ConsentIpAddress).HasMaxLength(45); // IPv6 max length

            builder.HasMany(c => c.Requests)
                .WithOne(r => r.Client)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Deals)
                .WithOne(d => d.Client)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Documents)
                .WithOne(d => d.Client)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы для поиска
            builder.HasIndex(c => c.Phone);
            builder.HasIndex(c => c.Email);
            builder.HasIndex(c => c.Source);
            builder.HasIndex(c => c.CreatedAt);

            // НОВЫЕ ИНДЕКСЫ ДЛЯ ЛК
            builder.HasIndex(c => c.AccountLogin);
            builder.HasIndex(c => c.HasPersonalAccount);
            builder.HasIndex(c => c.IsAccountActive);
        }
    }
}