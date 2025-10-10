using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.Application.Services
{
    public class DealHistoryService : IDealHistoryService
    {
        private readonly IDealHistoryRepository _historyRepository;

        public DealHistoryService(IDealHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<Guid> AddHistoryRecord(DealHistory history)
        {
            return await _historyRepository.Create(history);
        }

        public async Task<TimeSpan> GetAverageStageTime(Guid stageId)
        {
            return await _historyRepository.GetAverageTimeInStage(stageId);
        }

        public async Task<List<DealHistory>> GetDealHistory(Guid dealId)
        {
            return await _historyRepository.GetByDealId(dealId);
        }

        public async Task<List<DealHistory>> GetHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            var allHistory = await _historyRepository.GetRecentHistory(1000); // Ограничиваем для производительности
            return allHistory
                .Where(h => h.ChangedAt >= startDate && h.ChangedAt <= endDate)
                .ToList();
        }

        public async Task<List<DealHistory>> GetRecentActivity(int count = 10)
        {
            return await _historyRepository.GetRecentHistory(count);
        }
    }
}
