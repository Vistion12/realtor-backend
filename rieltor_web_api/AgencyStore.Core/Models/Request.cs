namespace AgencyStore.Core.Models
{
    public class Request
    {
        public const int MAX_MESSAGE_LENGTH = 2000;

        private Request(Guid id, Guid clientId, Guid? propertyId, string type,
                       string status, string message, DateTime createdAt)
        {
            Id = id;
            ClientId = clientId;
            PropertyId = propertyId;
            Type = type;
            Status = status;
            Message = message;
            CreatedAt = createdAt;
        }

        public Guid Id { get; }
        public Guid ClientId { get; }
        public Guid? PropertyId { get; } // null для консультаций
        public string Type { get; } // consultation, viewing, callback
        public string Status { get; } // new, in_progress, completed
        public string Message { get; } // JSON с данными формы
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public static (Request request, string error) Create(Guid id, Guid clientId,
            Guid? propertyId, string type, string status, string message, DateTime createdAt)
        {
            var error = string.Empty;

            // Валидация clientId
            if (clientId == Guid.Empty)
            {
                error = "Client ID cannot be empty";
            }

            // Валидация типа
            var validTypes = new[] { "consultation", "viewing", "callback" };
            if (string.IsNullOrEmpty(type) || !validTypes.Contains(type.ToLower()))
            {
                error = "Invalid request type";
            }

            // Валидация статуса
            var validStatuses = new[] { "new", "in_progress", "completed" };
            if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status.ToLower()))
            {
                error = "Invalid status";
            }

            // Валидация сообщения
            if (string.IsNullOrEmpty(message))
            {
                error = "Message cannot be empty";
            }
            else if (message.Length > MAX_MESSAGE_LENGTH)
            {
                error = $"Message cannot be longer than {MAX_MESSAGE_LENGTH} symbols";
            }

            var request = new Request(id, clientId, propertyId, type, status, message, createdAt);
            return (request, error);
        }

    }
}
