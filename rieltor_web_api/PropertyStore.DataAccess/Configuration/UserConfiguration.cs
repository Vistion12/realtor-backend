using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            
            builder.HasIndex(u => u.Username)
                .IsUnique();

            
        }
    }
}
