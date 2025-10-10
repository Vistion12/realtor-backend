using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Entities
{
    public class DealStageEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public TimeSpan ExpectedDuration { get; set; }
        public Guid PipelineId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public DealPipelineEntity Pipeline { get; set; } = null!;
        public List<DealEntity> Deals { get; set; } = new();
    }
}
