
namespace AgencyStore.Core.Models
{
    public class ClientDocument
    {
        public const int MAX_FILENAME_LENGTH = 255;
        public const int MAX_FILEPATH_LENGTH = 500;
        public const int MAX_FILETYPE_LENGTH = 100;
        public const int MAX_CATEGORY_LENGTH = 50;
        public const int MAX_DESCRIPTION_LENGTH = 500;

        private ClientDocument(Guid id, Guid clientId, string fileName, string filePath,
                              string fileUrl, long fileSize, string fileType, string category,
Guid? dealId, string uploadedBy, Guid? uploadedById, string? description,
                              bool isTemplate, bool isRequired, DateTime? requiredUntil)
        {
            Id = id;
            ClientId = clientId;
            FileName = fileName;
            FilePath = filePath;
            FileUrl = fileUrl;
            FileSize = fileSize;
            FileType = fileType;
            Category = category;
            DealId = dealId;
            UploadedBy = uploadedBy;
            UploadedById = uploadedById;
            Description = description;
            IsTemplate = isTemplate;
            IsRequired = isRequired;
            RequiredUntil = requiredUntil;
            UploadedAt = DateTime.UtcNow;
        }

        public Guid Id { get; }
        public Guid ClientId { get; }
        public Client Client { get; } = null!;
        public Guid? DealId { get; } // Привязка к сделке (опционально)
        public Deal? Deal { get; }

        // Метаданные файла
        public string FileName { get; } = string.Empty;
        public string FilePath { get; } = string.Empty;  // Путь в storage
        public string FileUrl { get; } = string.Empty;   // URL для скачивания
        public long FileSize { get; }
        public string FileType { get; } = string.Empty;  // MIME type
        public string Category { get; } = "general";     // "contract", "passport", "template"

        // Информация о загрузке
        public string UploadedBy { get; } = string.Empty; // "client" или "realtor"
        public Guid? UploadedById { get; }               // ID пользователя
        public DateTime UploadedAt { get; } = DateTime.UtcNow;
        public string? Description { get; }

        // Для шаблонов от риелтора
        public bool IsTemplate { get; } = false;
        public bool IsRequired { get; } = false;         // Обязательный документ
        public DateTime? RequiredUntil { get; }          // Срок предоставления

        public static (ClientDocument document, string error) Create(
            Guid id,
            Guid clientId,
            string fileName,
            string filePath,
            string fileUrl,
            long fileSize,
            string fileType,
            string category = "general",
            Guid? dealId = null,
            string uploadedBy = "client",
            Guid? uploadedById = null,
            string? description = null,
            bool isTemplate = false,
            bool isRequired = false,
            DateTime? requiredUntil = null)
        {
            var error = string.Empty;

            // Валидация обязательных полей
            if (clientId == Guid.Empty)
                error = "Client ID cannot be empty";
            else if (string.IsNullOrEmpty(fileName))
                error = "File name cannot be empty";
            else if (fileName.Length > MAX_FILENAME_LENGTH)
                error = $"File name cannot be longer than {MAX_FILENAME_LENGTH} symbols";
            else if (string.IsNullOrEmpty(filePath))
                error = "File path cannot be empty";
            else if (filePath.Length > MAX_FILEPATH_LENGTH)
                error = $"File path cannot be longer than {MAX_FILEPATH_LENGTH} symbols";
            else if (string.IsNullOrEmpty(fileType))
                error = "File type cannot be empty";
            else if (fileType.Length > MAX_FILETYPE_LENGTH)
                error = $"File type cannot be longer than {MAX_FILETYPE_LENGTH} symbols";
            else if (category.Length > MAX_CATEGORY_LENGTH)
                error = $"Category cannot be longer than {MAX_CATEGORY_LENGTH} symbols";
            else if (description?.Length > MAX_DESCRIPTION_LENGTH)
                error = $"Description cannot be longer than {MAX_DESCRIPTION_LENGTH} symbols";

            // Валидация категорий
            var validCategories = new[] { "general", "contract", "passport", "template", "other" };
            if (!validCategories.Contains(category.ToLower()))
                error = "Invalid document category";

            // Валидация uploadedBy
            var validUploaders = new[] { "client", "realtor" };
            if (!validUploaders.Contains(uploadedBy.ToLower()))
                error = "Invalid uploader type";

            if (!string.IsNullOrEmpty(error))
                return (null!, error);

            var document = new ClientDocument(
                id, clientId, fileName, filePath, fileUrl, fileSize, fileType, category,
                dealId, uploadedBy, uploadedById, description, isTemplate, isRequired, requiredUntil);

            return (document, error);
        }

        // Метод для проверки просроченности обязательного документа
        public bool IsOverdue()
        {
            return IsRequired && RequiredUntil.HasValue && DateTime.UtcNow > RequiredUntil.Value;
        }
    }
}
