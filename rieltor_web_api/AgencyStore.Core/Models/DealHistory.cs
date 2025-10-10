
namespace AgencyStore.Core.Models
{
    public class DealHistory
    {
        public Guid Id { get; set; }

        // Внешние ключи
        public Guid DealId { get; set; }
        public Deal Deal { get; set; } = null!;

        public Guid FromStageId { get; set; }
        public DealStage FromStage { get; set; } = null!;

        public Guid ToStageId { get; set; }
        public DealStage ToStage { get; set; } = null!;

        public string? Notes { get; set; } // Комментарий к переходу

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan TimeInStage { get; set; } // Сколько времени провели на предыдущем этапе

        public static DealHistory Create(
            Guid id,
            Guid dealId,
            Guid fromStageId,
            Guid toStageId,
            TimeSpan timeInStage,
            string? notes = null)
        {
            return new DealHistory
            {
                Id = id,
                DealId = dealId,
                FromStageId = fromStageId,
                ToStageId = toStageId,
                Notes = notes,
                TimeInStage = timeInStage,
                ChangedAt = DateTime.UtcNow
            };
        }

    }
}
