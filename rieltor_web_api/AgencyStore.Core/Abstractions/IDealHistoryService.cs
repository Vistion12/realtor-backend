using AgencyStore.Core.Models;


namespace AgencyStore.Core.Abstractions
{
    public interface IDealHistoryService
    {
        Task<Guid> AddHistoryRecord(DealHistory history);
        Task<List<DealHistory>> GetDealHistory(Guid dealId);
        Task<List<DealHistory>> GetRecentActivity(int count = 10);
        Task<TimeSpan> GetAverageStageTime(Guid stageId);
        Task<List<DealHistory>> GetHistoryByDateRange(DateTime startDate, DateTime endDate);
    }
}
