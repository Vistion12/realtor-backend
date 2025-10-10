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

                var (success, error) = deal.MoveToStage(newStage, notes);
                if (success)
                {
                    await _dealRepository.Update(deal);
                    _logger.LogInformation("Сделка {DealId} перемещена на этап {StageName}", dealId, newStage.Name);
                }

                return (success, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при перемещении сделки {DealId} на этап {NewStageId}", dealId, newStageId);
                return (false, $"Ошибка при перемещении сделки: {ex.Message}");
            }
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
    }
}
