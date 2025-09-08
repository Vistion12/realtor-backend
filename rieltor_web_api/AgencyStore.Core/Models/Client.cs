namespace AgencyStore.Core.Models
{
    public class Client
    {
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_PHONE_LENGTH = 20;
        public const int MAX_EMAIL_LENGTH = 100;
        public const int MAX_NOTES_LENGTH = 1000;

        private Client(Guid id, string name, string phone, string? email,
                      string source, string? notes, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Phone = phone;
            Email = email;
            Source = source;
            Notes = notes;
            CreatedAt = createdAt;
        }

        public Guid Id { get; }
        public string Name { get; } = string.Empty;
        public string Phone { get; } = string.Empty;
        public string? Email { get; }
        public string Source { get; } = string.Empty; // website, telegram, phone_call
        public string? Notes { get; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        private readonly List<Request> _requests = new();
        public IReadOnlyList<Request> Requests => _requests.AsReadOnly();

        public void AddRequest(Request request)
        {
            if (request.ClientId != Id)
                throw new ArgumentException("Request does not belong to this client");

            _requests.Add(request);
        }

        public static (Client client, string error) Create(Guid id, string name, string phone,
            string? email, string source, string? notes, DateTime createdAt)
        {
            var error = string.Empty;

            // Валидация имени
            if (string.IsNullOrEmpty(name))
            {
                error = "Name cannot be empty";
            }
            else if (name.Length > MAX_NAME_LENGTH)
            {
                error = $"Name cannot be longer than {MAX_NAME_LENGTH} symbols";
            }

            // Валидация телефона
            if (string.IsNullOrEmpty(phone))
            {
                error = "Phone cannot be empty";
            }
            else if (phone.Length > MAX_PHONE_LENGTH)
            {
                error = $"Phone cannot be longer than {MAX_PHONE_LENGTH} symbols";
            }

            // Валидация email
            if (!string.IsNullOrEmpty(email) && email.Length > MAX_EMAIL_LENGTH)
            {
                error = $"Email cannot be longer than {MAX_EMAIL_LENGTH} symbols";
            }

            // Валидация источника
            var validSources = new[] { "website", "telegram", "phone_call" };
            if (string.IsNullOrEmpty(source) || !validSources.Contains(source.ToLower()))
            {
                error = "Invalid source";
            }

            // Валидация заметок
            if (!string.IsNullOrEmpty(notes) && notes.Length > MAX_NOTES_LENGTH)
            {
                error = $"Notes cannot be longer than {MAX_NOTES_LENGTH} symbols";
            }

            var client = new Client(id, name, phone, email, source, notes, createdAt);
            return (client, error);
        }
    }
}
