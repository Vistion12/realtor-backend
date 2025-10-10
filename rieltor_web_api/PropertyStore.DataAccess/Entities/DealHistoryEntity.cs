using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Entities
{
    public class DealHistoryEntity
    {
        public Guid Id { get; set; }

        // Внешние ключи
        public Guid DealId { get; set; }
        public Guid FromStageId { get; set; }
        public Guid ToStageId { get; set; }

        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan TimeInStage { get; set; }

        // Навигационные свойства
        public DealEntity Deal { get; set; } = null!;
        public DealStageEntity FromStage { get; set; } = null!;
        public DealStageEntity ToStage { get; set; } = null!;
    }
}
