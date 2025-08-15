namespace AgencyStore.Core.Models
{
    public class Property //бизнес модель
    {
        public const int MAX_TITLE_LENGTH = 250;
        private Property(Guid id, string title, string type, decimal price, string address, decimal area,
            int rooms, string description, bool isActive, DateTime createdAt)
        {
            Id = id;
            Title = title;
            Type = type;
            Price = price;
            Address = address;
            Area = area;
            Rooms = rooms;
            Description = description;
            IsActive = isActive;
            CreatedAt = createdAt;
        }
        public Guid Id { get; }
        public string Title { get; } = string.Empty;

        public string Type { get; } = string.Empty; // novostroyki, secondary, rent, countryside, invest

        public decimal Price { get; }

        public string Address { get; } = string.Empty;

        public decimal Area { get; }

        public int Rooms { get; }
        public string Description { get; } = string.Empty;
        public string MainPhotoUrl { get; } = string.Empty;

        public bool IsActive { get; } = true;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public static (Property property, string Error) Create(Guid id, string title, string type, decimal price, string address,
                        decimal area, int rooms, string description, bool isActive, DateTime createdAt)
        {
            var error = string.Empty;

            // Валидация title
            if (string.IsNullOrEmpty(title))
            {
                error="Title cannot be empty";
            }
            else if (title.Length > MAX_TITLE_LENGTH)
            {
                error = $"Title cannot be longer than {MAX_TITLE_LENGTH} symbols";
            }

            // Валидация type
            var validTypes = new[] { "novostroyki", "secondary", "rent", "countryside", "invest" };
            if (string.IsNullOrEmpty(type) || !validTypes.Contains(type.ToLower()))
            {
                error = "Invalid property type";
            }

            // Валидация price
            if (price <= 0)
            {
                error = "Price must be greater than 0";
            }

            // Валидация address
            if (string.IsNullOrEmpty(address))
            {
                error = "Address cannot be empty";
            }

            // Валидация area
            if (area <= 0)
            {
                error = "Area must be greater than 0";
            }

            // Валидация rooms
            if (rooms <= 0)
            {
                error = "Rooms count must be greater than 0";
            }

            // Валидация description (необязательное поле, но если есть - проверяем длину)
            if (!string.IsNullOrEmpty(description) && description.Length > 2000)
            {
                error = "Description cannot be longer than 2000 symbols";
            }

            

            var property = new Property(
                id, title, type, price, address,
                area, rooms, description, isActive, createdAt);

            return (property, error);
        }
    }
}
