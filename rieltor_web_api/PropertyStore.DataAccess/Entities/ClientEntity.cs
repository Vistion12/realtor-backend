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
        public DateTime CreatedAt { get; set; }

        // НОВЫЕ ПОЛЯ ДЛЯ ЛК
        public bool HasPersonalAccount { get; set; } = false;
        public string? AccountLogin { get; set; }
        public string? TemporaryPassword { get; set; }
        public bool IsAccountActive { get; set; } = false;
        public bool ConsentToPersonalData { get; set; } = false;
        public DateTime? ConsentGivenAt { get; set; }
        public string? ConsentIpAddress { get; set; }

        // Навигационные свойства
        public List<RequestEntity> Requests { get; set; } = new();
        public List<DealEntity> Deals { get; set; } = new();
        public List<ClientDocumentEntity> Documents { get; set; } = new();
    }
}