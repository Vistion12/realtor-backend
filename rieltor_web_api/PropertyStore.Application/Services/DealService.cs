using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.Application.Services
{
    public class DealService : IDealService
    {
        private readonly IDealRepository _dealRepository;
        private readonly IDealStageRepository _stageRepository;
        private readonly IDealHistoryRepository _historyRepository;
        private readonly IClientsRepository _clientsRepository;
        private readonly ILogger<DealService> _logger;

        public DealService(
            IDealRepository dealRepository,
            IDealStageRepository stageRepository,
            IDealHistoryRepository historyRepository,
            IClientsRepository clientsRepository,
            ILogger<DealService> logger)
        {
            _dealRepository = dealRepository;
            _stageRepository = stageRepository;
            _historyRepository = historyRepository;
            _clientsRepository = clientsRepository;
            _logger = logger;
        }

        public async Task<Guid> CreateDeal(Deal deal)
        {
            // Проверяем существование клиента
            var client = await _clientsRepository.GetById(deal.ClientId);
            if (client == null)
                throw new ArgumentException("Клиент не найден");

            // Загружаем этап для расчета дедлайна
            var stage = await _stageRepository.GetById(deal.CurrentStageId);
            if (stage == null)
                throw new ArgumentException("Этап не найден");

            // Рассчитываем дедлайн
            deal.CalculateStageDeadline(stage.ExpectedDuration);

            return await _dealRepository.Create(deal);
        }

        public async Task<bool> CloseDeal(Guid dealId)
        {
            var deal = await _dealRepository.GetById(dealId);
            if (deal == null)
                return false;

            deal.CloseDeal();
            await _dealRepository.Update(deal);
            return true;
        }

        public async Task<Guid> DeleteDeal(Guid id)
        {
            return await _dealRepository.Delete(id);
        }

        public async Task<List<Deal>> GetAllDeals()
        {
            return await _dealRepository.Get();
        }

        public async Task<List<Deal>> GetActiveDeals()
        {
            return await _dealRepository.GetActiveDeals();
        }

        public async Task<Deal?> GetDealById(Guid id)
        {
            return await _dealRepository.GetById(id);
        }

        public async Task<Deal?> GetDealWithDetails(Guid id)
        {
            return await _dealRepository.GetByIdWithDetails(id);
        }

        public async Task<List<Deal>> GetDealsByClient(Guid clientId)
        {
            return await _dealRepository.GetByClientId(clientId);
        }

        public async Task<List<Deal>> GetDealsByPipeline(Guid pipelineId)
        {
            return await _dealRepository.GetByPipelineId(pipelineId);
        }

        public async Task<List<Deal>> GetDealsByStage(Guid stageId)
        {
            return await _dealRepository.GetByStageId(stageId);
        }

        public async Task<DealAnalytics> GetDealAnalytics(Guid pipelineId)
        {
            var deals = await _dealRepository.GetByPipelineId(pipelineId);

            var totalDeals = deals.Count;
            var activeDeals = deals.Count(d => d.IsActive);
            var completedDeals = deals.Count(d => !d.IsActive);

            var totalDealAmount = deals.Where(d => d.DealAmount.HasValue).Sum(d => d.DealAmount.Value);
            var averageDealAmount = activeDeals > 0 ? totalDealAmount / activeDeals : 0;

            // Упрощенный расчет средней продолжительности
            var averageDuration = TimeSpan.FromDays(30); // Заглушка

            return new DealAnalytics(
                totalDeals,
                activeDeals,
                completedDeals,
                totalDealAmount,
                averageDealAmount,
                averageDuration
            );
        }

        public async Task<List<Deal>> GetOverdueDeals()
        {
            return await _dealRepository.GetOverdueDeals();
        }

        public async Task<List<DealStageAnalytics>> GetPipelineAnalytics(Guid pipelineId)
        {
            var stages = await _stageRepository.GetByPipelineId(pipelineId);
            var result = new List<DealStageAnalytics>();

            foreach (var stage in stages)
            {
                var dealsInStage = await _dealRepository.GetByStageId(stage.Id);
                var overdueDeals = dealsInStage.Count(d => d.IsOverdue());
                var averageTime = await _historyRepository.GetAverageTimeInStage(stage.Id);

                result.Add(new DealStageAnalytics(
                    stage.Id,
                    stage.Name,
                    dealsInStage.Count,
                    averageTime,
                    overdueDeals
                ));
            }

            return result;
        }

        public async Task<(bool success, string error)> MoveDealToStage(Guid dealId, Guid newStageId, string? notes = null)
        {
            try
            {
                var deal = await _dealRepository.GetById(dealId);
                if (deal == null)
                    return (false, "Сделка не найдена");

                var newStage = await _stageRepository.GetById(newStageId);
                if (newStage == null)
                    return (false, "Этап не найден");

                // 1. Проверка принадлежности к одной воронке
                if (newStage.PipelineId != deal.PipelineId)
                    return (false, "Этап не принадлежит текущей воронке сделки");

                // 2. Проверка что это не тот же этап
                if (newStage.Id == deal.CurrentStageId)
                    return (false, "Сделка уже находится на этом этапе");

                // 3. Проверка бизнес-правил переходов
                var validationResult = await ValidateStageTransition(deal.CurrentStageId, newStage.Id);
                if (!validationResult.IsValid)
                    return (false, validationResult.ErrorMessage);

                var (success, error) = deal.MoveToStage(newStage, notes);
                if (success)
                {
                    await _dealRepository.Update(deal);
                    _logger.LogInformation("Сделка {DealId} перемещена с этапа {FromStage} на этап {ToStage}",
                        dealId, deal.CurrentStage?.Name, newStage.Name);
                }

                return (success, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при перемещении сделки {DealId} на этап {NewStageId}", dealId, newStageId);
                return (false, $"Ошибка при перемещении сделки: {ex.Message}");
            }
        }


        private async Task<(bool IsValid, string ErrorMessage)> ValidateStageTransition(Guid fromStageId, Guid toStageId)
        {
            var fromStage = await _stageRepository.GetById(fromStageId);
            var toStage = await _stageRepository.GetById(toStageId);

            if (fromStage == null || toStage == null)
                return (false, "Этапы не найдены");

            // Правило: нельзя перескакивать через этапы (можно отключить если нужно)
            if (Math.Abs(fromStage.Order - toStage.Order) > 1)
                return (false, "Нельзя пропускать этапы воронки. Переход только на соседние этапы.");

            // Правило: нельзя двигаться назад без особой причины (можно настроить)
            if (toStage.Order < fromStage.Order)
            {
                // Можно добавить логику для разрешенных откатов
                // Пока разрешаем, но логируем
                _logger.LogWarning("Обратный переход с этапа {FromStage} на {ToStage}", fromStage.Name, toStage.Name);
            }

            return (true, string.Empty);
        }


        public async Task<bool> ReopenDeal(Guid dealId)
        {
            var deal = await _dealRepository.GetById(dealId);
            if (deal == null)
                return false;

            deal.IsActive = true;
            deal.ClosedAt = null;
            deal.UpdatedAt = DateTime.UtcNow;

            await _dealRepository.Update(deal);
            return true;
        }

        public async Task<Guid> UpdateDeal(Deal deal)
        {
            var existing = await _dealRepository.GetById(deal.Id);
            if (existing == null)
                throw new ArgumentException("Сделка не найдена");

            return await _dealRepository.Update(deal);
        }

        public async Task<List<PropertyTypeAnalytics>> GetPropertyTypeAnalytics(Guid pipelineId)
        {
            _logger.LogInformation("Starting property type analytics for pipeline {PipelineId}", pipelineId);

            var deals = await _dealRepository.GetByPipelineId(pipelineId);
            _logger.LogInformation("Found {DealCount} deals for pipeline", deals.Count);

            // ПРИОРИТЕТ 1: Анализ по привязанным объектам недвижимости
            var dealsWithProperty = deals.Where(d => d.PropertyId.HasValue && d.Property != null).ToList();

            if (dealsWithProperty.Any())
            {
                _logger.LogInformation("Analyzing {Count} deals with property objects", dealsWithProperty.Count);

                var typeGroups = dealsWithProperty
                    .GroupBy(d => d.Property!.Type)
                    .Select(g => new
                    {
                        PropertyType = g.Key,
                        DealCount = g.Count()
                    })
                    .ToList();

                var totalDealsWithProperty = typeGroups.Sum(g => g.DealCount);
                var totalAllDeals = deals.Count;

                // Маппинг технических названий на человекочитаемые
                var typeDisplayNames = new Dictionary<string, string>
        {
            { "novostroyki", "Новостройки" },
            { "secondary", "Вторичное жилье" },
            { "rent", "Аренда" },
            { "countryside", "Загородная недвижимость" },
            { "invest", "Инвестиционные объекты" }
        };

                var result = typeGroups.Select(g => new PropertyTypeAnalytics(
                    PropertyType: g.PropertyType,
                    DisplayName: typeDisplayNames.GetValueOrDefault(g.PropertyType, g.PropertyType),
                    DealCount: g.DealCount,
                    Percentage: totalAllDeals > 0 ? Math.Round((double)g.DealCount / totalAllDeals * 100, 1) : 0
                )).ToList();

                // Добавляем категорию "Без привязки" для сделок без недвижимости
                var dealsWithoutProperty = deals.Count(d => !d.PropertyId.HasValue || d.Property == null);
                if (dealsWithoutProperty > 0)
                {
                    result.Add(new PropertyTypeAnalytics(
                        PropertyType: "none",
                        DisplayName: "Без привязки к объекту",
                        DealCount: dealsWithoutProperty,
                        Percentage: totalAllDeals > 0 ? Math.Round((double)dealsWithoutProperty / totalAllDeals * 100, 1) : 100
                    ));
                }

                _logger.LogInformation(" Returning {ResultCount} analytics items from property data", result.Count);
                return result;
            }
            else
            {
                // ПРИОРИТЕТ 2: Fallback - анализ по названиям сделок (для демонстрации)
                _logger.LogInformation(" No properties found, using fallback title analysis");

                var typeGroups = AnalyzeDealsByTitle(deals);
                var totalAllDeals = deals.Count;

                var result = typeGroups.Select(g => new PropertyTypeAnalytics(
                    PropertyType: g.PropertyType,
                    DisplayName: g.DisplayName,
                    DealCount: g.DealCount,
                    Percentage: totalAllDeals > 0 ? Math.Round((double)g.DealCount / totalAllDeals * 100, 1) : 0
                )).ToList();

                _logger.LogInformation("✅ Returning {ResultCount} analytics items from title analysis", result.Count);
                return result;
            }
        }

        // Fallback метод для анализа по названиям сделок
        private List<(string PropertyType, string DisplayName, int DealCount)> AnalyzeDealsByTitle(List<Deal> deals)
        {
            var typeGroups = new Dictionary<string, (string DisplayName, int Count)>
                {
                    { "novostroyki", ("Новостройки", 0) },
                    { "secondary", ("Вторичное жилье", 0) },
                    { "rent", ("Аренда", 0) },
                    { "countryside", ("Загородная недвижимость", 0) },
                    { "invest", ("Инвестиционные объекты", 0) },
                    { "other", ("Другое", 0) }
                };

            foreach (var deal in deals)
            {
                var title = deal.Title.ToLower();
                string detectedType = "other";

                if (title.Contains("новостр") || title.Contains("novostroyki") || title.Contains("новая"))
                    detectedType = "novostroyki";
                else if (title.Contains("вторич") || title.Contains("втор") || title.Contains("secondary") || title.Contains("вторичное"))
                    detectedType = "secondary";
                else if (title.Contains("аренд") || title.Contains("rent") || title.Contains("снять") || title.Contains("сдам"))
                    detectedType = "rent";
                else if (title.Contains("загород") || title.Contains("дача") || title.Contains("коттедж") || title.Contains("дом") || title.Contains("countryside"))
                    detectedType = "countryside";
                else if (title.Contains("инвест") || title.Contains("invest") || title.Contains("доход") || title.Contains("прибыль"))
                    detectedType = "invest";

                var current = typeGroups[detectedType];
                typeGroups[detectedType] = (current.DisplayName, current.Count + 1);
            }

            // Убираем пустые категории
            return typeGroups
                .Where(x => x.Value.Count > 0)
                .Select(x => (x.Key, x.Value.DisplayName, x.Value.Count))
                .ToList();
        }
    }
}
