
using System.ComponentModel.DataAnnotations;

namespace AgencyStore.Core.Models
{
    public class DealStage
    {
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_DESCRIPTION_LENGTH = 500;

        public Guid Id { get; set; }

        [Required]
        [MaxLength(MAX_NAME_LENGTH)]
        public string Name { get; set; } = string.Empty; // "Первичный контакт", "Осмотр объекта" и т.д.

        [MaxLength(MAX_DESCRIPTION_LENGTH)]
        public string? Description { get; set; }

        public int Order { get; set; } // Порядок этапа в воронке
        public TimeSpan ExpectedDuration { get; set; } // Ожидаемое время на этап

        // Внешний ключ
        public Guid PipelineId { get; set; }
        public DealPipeline Pipeline { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public List<Deal> Deals { get; set; } = new();

        public static (DealStage stage, string error) Create(
            Guid id,
            string name,
            int order,
            TimeSpan expectedDuration,
            Guid pipelineId,
            string? description = null)
        {
            var error = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                error = "Название этапа не может быть пустым";
            else if (name.Length > MAX_NAME_LENGTH)
                error = $"Название этапа не может превышать {MAX_NAME_LENGTH} символов";
            else if (description?.Length > MAX_DESCRIPTION_LENGTH)
                error = $"Описание не может превышать {MAX_DESCRIPTION_LENGTH} символов";
            else if (order < 0)
                error = "Порядок этапа не может быть отрицательным";
            else if (expectedDuration.TotalHours < 0)
                error = "Ожидаемая продолжительность не может быть отрицательной";

            var stage = new DealStage
            {
                Id = id,
                Name = name.Trim(),
                Description = description?.Trim(),
                Order = order,
                ExpectedDuration = expectedDuration,
                PipelineId = pipelineId,
                CreatedAt = DateTime.UtcNow
            };

            return (stage, error);
        }

    }
}
