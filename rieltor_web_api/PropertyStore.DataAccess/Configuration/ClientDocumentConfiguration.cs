using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Configuration
{
    public class ClientDocumentConfiguration : IEntityTypeConfiguration<ClientDocumentEntity>
    {
        public void Configure(EntityTypeBuilder<ClientDocumentEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(d => d.FileName)
                .HasMaxLength(ClientDocument.MAX_FILENAME_LENGTH)
                .IsRequired();

            builder.Property(d => d.FilePath)
                .HasMaxLength(ClientDocument.MAX_FILEPATH_LENGTH)
                .IsRequired();

            builder.Property(d => d.FileUrl)
                .HasMaxLength(ClientDocument.MAX_FILEPATH_LENGTH)
                .IsRequired();

            builder.Property(d => d.FileType)
                .HasMaxLength(ClientDocument.MAX_FILETYPE_LENGTH)
                .IsRequired();

            builder.Property(d => d.Category)
                .HasMaxLength(ClientDocument.MAX_CATEGORY_LENGTH)
                .HasDefaultValue("general");

            builder.Property(d => d.UploadedBy)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(d => d.Description)
                .HasMaxLength(ClientDocument.MAX_DESCRIPTION_LENGTH);

            builder.Property(d => d.FileSize)
                .IsRequired();

            builder.Property(d => d.UploadedAt)
                .IsRequired();

            builder.Property(d => d.IsTemplate)
                .HasDefaultValue(false);

            builder.Property(d => d.IsRequired)
                .HasDefaultValue(false);

            // Индексы для поиска
            builder.HasIndex(d => d.ClientId);
            builder.HasIndex(d => d.DealId);
            builder.HasIndex(d => d.Category);
            builder.HasIndex(d => d.UploadedBy);
            builder.HasIndex(d => d.UploadedAt);
            builder.HasIndex(d => d.IsRequired);
            builder.HasIndex(d => d.RequiredUntil);

            // Внешние ключи
            builder.HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Deal)
                .WithMany()
                .HasForeignKey(d => d.DealId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}