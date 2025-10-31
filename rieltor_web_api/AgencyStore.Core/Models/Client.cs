namespace AgencyStore.Core.Models
{
    public class Client
    {
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_PHONE_LENGTH = 20;
        public const int MAX_EMAIL_LENGTH = 100;
        public const int MAX_NOTES_LENGTH = 1000;
        public const int MAX_TEMP_PASSWORD_LENGTH = 100;

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

        public bool HasPersonalAccount { get; private set; } = false;
        public string? AccountLogin { get; private set; } // Email как логин
        public string? TemporaryPassword { get; private set; }
        public bool IsAccountActive { get; private set; } = false;
        public bool ConsentToPersonalData { get; private set; } = false;
        public DateTime? ConsentGivenAt { get; private set; }
        public string? ConsentIpAddress { get; private set; }



        private readonly List<Request> _requests = new();
        public IReadOnlyList<Request> Requests => _requests.AsReadOnly();

        private readonly List<Deal> _deals = new();
        public IReadOnlyList<Deal> Deals => _deals.AsReadOnly();

        public void AddDeal(Deal deal)
        {
            _deals.Add(deal);
        }

        public IEnumerable<Deal> GetActiveDeals()
        {
            return _deals.Where(d => d.IsActive);
        }

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

        public void ActivatePersonalAccount(string accountLogin, string temporaryPassword)
        {
            if (string.IsNullOrEmpty(accountLogin))
                throw new ArgumentException("Account login cannot be empty");

            if (string.IsNullOrEmpty(temporaryPassword))
                throw new ArgumentException("Temporary password cannot be empty");

            AccountLogin = accountLogin;
            TemporaryPassword = temporaryPassword;
            HasPersonalAccount = true;
            IsAccountActive = true;
        }

        public void GiveConsent(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException("IP address cannot be empty");

            ConsentToPersonalData = true;
            ConsentGivenAt = DateTime.UtcNow;
            ConsentIpAddress = ipAddress;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrEmpty(newPasswordHash))
                throw new ArgumentException("Password cannot be empty");

            TemporaryPassword = newPasswordHash;
            // После смены пароля временный пароль становится постоянным
        }

        public void DeactivateAccount()
        {
            IsAccountActive = false;
            TemporaryPassword = null; // Очищаем пароль при деактивации
        }

        public void SetPersonalAccountFields(
        bool hasPersonalAccount,
        string? accountLogin,
        string? temporaryPassword,
        bool isAccountActive)
        {
            HasPersonalAccount = hasPersonalAccount;
            AccountLogin = accountLogin;
            TemporaryPassword = temporaryPassword;
            IsAccountActive = isAccountActive;
        }

        public void SetConsentFields(bool consentToPersonalData, DateTime? consentGivenAt, string? consentIpAddress)
        {
            ConsentToPersonalData = consentToPersonalData;
            ConsentGivenAt = consentGivenAt;
            ConsentIpAddress = consentIpAddress;
        }

        // Или можно сделать один метод для всех полей ЛК
        public void SetClientAccountInfo(
            bool hasPersonalAccount = false,
            string? accountLogin = null,
            string? temporaryPassword = null,
            bool isAccountActive = false,
            bool consentToPersonalData = false,
            DateTime? consentGivenAt = null,
            string? consentIpAddress = null)
        {
            HasPersonalAccount = hasPersonalAccount;
            AccountLogin = accountLogin;
            TemporaryPassword = temporaryPassword;
            IsAccountActive = isAccountActive;
            ConsentToPersonalData = consentToPersonalData;
            ConsentGivenAt = consentGivenAt;
            ConsentIpAddress = consentIpAddress;
        }
    }
}
