namespace PropertyStore.DataAccess.Entities
{
    public class RequestEntity
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid? PropertyId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "new";
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ClientEntity Client { get; set; } = null!;
        public PropertyEntity? Property { get; set; }
    }
}
