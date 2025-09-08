namespace PropertyStore.DataAccess.Entities
{
    public class ClientEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<RequestEntity> Requests { get; set; } = new();
    }
}
