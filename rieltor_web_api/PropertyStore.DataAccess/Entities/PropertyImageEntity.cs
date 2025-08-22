

namespace PropertyStore.DataAccess.Entities
{
    public class PropertyImageEntity
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int Order { get; set; }

        // Навигационное свойство
        public PropertyEntity Property { get; set; } = null!;
    }
}
