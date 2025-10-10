

using System.ComponentModel.DataAnnotations;

namespace AgencyStore.Core.Models
{
    public class DealPipeline
    {
        public const int MAX_NAME_LENGTH = 100;
        public const int MAX_DESCRIPTION_LENGTH = 500;

        public Guid Id { get; set; }

        [Required]
        [MaxLength(MAX_NAME_LENGTH)]
        public string Name { get; set; } = string.Empty; // "Покупка", "Продажа", "Аренда"

        [MaxLength(MAX_DESCRIPTION_LENGTH)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        public List<DealStage> Stages { get; set; } = new();
        public List<Deal> Deals { get; set; } = new();

        public static (DealPipeline pipeline, string error) Create(
            Guid id,
            string name,
            string? description = null)
        {
            var error = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                error = "Название воронки не может быть пустым";
            else if (name.Length > MAX_NAME_LENGTH)
                error = $"Название воронки не может превышать {MAX_NAME_LENGTH} символов";
            else if (description?.Length > MAX_DESCRIPTION_LENGTH)
                error = $"Описание не может превышать {MAX_DESCRIPTION_LENGTH} символов";

            var pipeline = new DealPipeline
            {
                Id = id,
                Name = name.Trim(),
                Description = description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            return (pipeline, error);
        }
    }
}
