namespace PropertyStore.DataAccess.Entities
{
    public class ClientDocumentEntity
    {
        public Guid Id { get; set; }

        // Связи
        public Guid ClientId { get; set; }
        public ClientEntity Client { get; set; } = null!;
        public Guid? DealId { get; set; }
        public DealEntity? Deal { get; set; }

        // Метаданные файла
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public string Category { get; set; } = "general";

        // Информация о загрузке
        public string UploadedBy { get; set; } = string.Empty;
        public Guid? UploadedById { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }

        // Для шаблонов
        public bool IsTemplate { get; set; } = false;
        public bool IsRequired { get; set; } = false;
        public DateTime? RequiredUntil { get; set; }
    }
}