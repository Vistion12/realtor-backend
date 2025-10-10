using System.ComponentModel.DataAnnotations;

namespace AgencyStore.Core.Models
{
    public class Deal
    {
        public const int MAX_NOTES_LENGTH = 2000;

        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // "Сделка по покупке квартиры у Иванова"

        [MaxLength(MAX_NOTES_LENGTH)]
        public string? Notes { get; set; }

        public decimal? DealAmount { get; set; } // Сумма сделки
        public DateTime? ExpectedCloseDate { get; set; } // Ожидаемая дата закрытия

        // Текущий прогресс
        public Guid CurrentStageId { get; set; }
        public DealStage CurrentStage { get; set; } = null!;

        // Внешние ключи
        public Guid PipelineId { get; set; }
        public DealPipeline Pipeline { get; set; } = null!;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid? PropertyId { get; set; }
        public Property? Property { get; set; }

        public Guid? RequestId { get; set; }
        public Request? Request { get; set; }

        // Даты этапов
        public DateTime StageStartedAt { get; set; } = DateTime.UtcNow; // Когда перешли на текущий этап
        public DateTime? StageDeadline { get; set; } // Дедлайн текущего этапа

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; } // Дата завершения сделки

        public bool IsActive { get; set; } = true;

        // Навигационные свойства
        public List<DealHistory> History { get; set; } = new();

        public static (Deal deal, string error) Create(
            Guid id,
            string title,
            Guid clientId,
            Guid pipelineId,
            Guid currentStageId,
            Guid? propertyId = null,
            Guid? requestId = null,
            string? notes = null,
            decimal? dealAmount = null,
            DateTime? expectedCloseDate = null)
        {
            var error = string.Empty;

            if (string.IsNullOrWhiteSpace(title))
                error = "Название сделки не может быть пустым";
            else if (title.Length > 200)
                error = "Название сделки не может превышать 200 символов";
            else if (notes?.Length > MAX_NOTES_LENGTH)
                error = $"Заметки не могут превышать {MAX_NOTES_LENGTH} символов";

            var deal = new Deal
            {
                Id = id,
                Title = title.Trim(),
                ClientId = clientId,
                PipelineId = pipelineId,
                CurrentStageId = currentStageId,
                PropertyId = propertyId,
                RequestId = requestId,
                Notes = notes?.Trim(),
                DealAmount = dealAmount,
                ExpectedCloseDate = expectedCloseDate,
                StageStartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return (deal, error);
        }

        // ДОБАВЛЯЕМ МЕТОД ДЛЯ РАСЧЕТА ДЕДЛАЙНА ЭТАПА
        public void CalculateStageDeadline(TimeSpan stageExpectedDuration)
        {
            if (stageExpectedDuration > TimeSpan.Zero)
            {
                StageDeadline = StageStartedAt.Add(stageExpectedDuration);
            }
            // Если ExpectedDuration = 0, то дедлайн не устанавливаем
        }

        // ДОБАВЛЯЕМ МЕТОД ДЛЯ ПЕРЕХОДА НА НОВЫЙ ЭТАП
        public (bool success, string error) MoveToStage(DealStage newStage, string? notes = null)
        {
            if (newStage == null)
                return (false, "Этап не может быть null");

            if (newStage.PipelineId != PipelineId)
                return (false, "Этап не принадлежит текущей воронке сделки");

            // Создаем запись в истории
            var timeInCurrentStage = GetTimeInCurrentStage();
            var historyEntry = DealHistory.Create(
                Guid.NewGuid(),
                Id,
                CurrentStageId,
                newStage.Id,
                timeInCurrentStage,
                notes
            );

            History.Add(historyEntry);

            // Обновляем текущий этап
            var previousStageId = CurrentStageId;
            CurrentStageId = newStage.Id;
            CurrentStage = newStage;
            StageStartedAt = DateTime.UtcNow;

            // Пересчитываем дедлайн для нового этапа
            CalculateStageDeadline(newStage.ExpectedDuration);

            UpdatedAt = DateTime.UtcNow;

            return (true, string.Empty);
        }

        public bool IsOverdue()
        {
            return StageDeadline.HasValue && DateTime.UtcNow > StageDeadline.Value;
        }

        public TimeSpan GetTimeInCurrentStage()
        {
            return DateTime.UtcNow - StageStartedAt;
        }

        // ДОБАВЛЯЕМ МЕТОД ДЛЯ ЗАВЕРШЕНИЯ СДЕЛКИ
        public void CloseDeal()
        {
            IsActive = false;
            ClosedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}