

namespace PropertyStore.DataAccess.Entities
{
    public class DealPipelineEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        public List<DealStageEntity> Stages { get; set; } = new();
        public List<DealEntity> Deals { get; set; } = new();
    }
}
