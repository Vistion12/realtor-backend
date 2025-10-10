using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Entities
{
    public class DealEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? DealAmount { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }

        // Текущий прогресс
        public Guid CurrentStageId { get; set; }

        // Внешние ключи
        public Guid PipelineId { get; set; }
        public Guid ClientId { get; set; }
        public Guid? PropertyId { get; set; }
        public Guid? RequestId { get; set; }

        // Даты этапов
        public DateTime StageStartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StageDeadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public DealPipelineEntity Pipeline { get; set; } = null!;
        public ClientEntity Client { get; set; } = null!;
        public DealStageEntity CurrentStage { get; set; } = null!;
        public PropertyEntity? Property { get; set; }
        public RequestEntity? Request { get; set; }
        public List<DealHistoryEntity> History { get; set; } = new();
    }
}
