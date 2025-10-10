using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealHistoryRepository
    {
        Task<Guid> Create(DealHistory history);
        Task<List<DealHistory>> GetByDealId(Guid dealId);
        Task<List<DealHistory>> GetByStageId(Guid stageId);
        Task<TimeSpan> GetAverageTimeInStage(Guid stageId);
        Task<List<DealHistory>> GetRecentHistory(int count = 10);
    }
}
