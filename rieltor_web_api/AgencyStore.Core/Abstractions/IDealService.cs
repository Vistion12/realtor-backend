

using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealService
    {
        Task<Guid> CreateDeal(Deal deal);
        Task<Guid> UpdateDeal(Deal deal);
        Task<Guid> DeleteDeal(Guid id);
        Task<List<Deal>> GetAllDeals();
        Task<Deal?> GetDealById(Guid id);
        Task<Deal?> GetDealWithDetails(Guid id);
        Task<List<Deal>> GetDealsByClient(Guid clientId);
        Task<List<Deal>> GetDealsByPipeline(Guid pipelineId);
        Task<List<Deal>> GetDealsByStage(Guid stageId);
        Task<List<Deal>> GetActiveDeals();
        Task<List<Deal>> GetOverdueDeals();

        // Основные бизнес-операции
        Task<(bool success, string error)> MoveDealToStage(Guid dealId, Guid newStageId, string? notes = null);
        Task<bool> CloseDeal(Guid dealId);
        Task<bool> ReopenDeal(Guid dealId);

        // Аналитика
        Task<DealAnalytics> GetDealAnalytics(Guid pipelineId);
        Task<List<DealStageAnalytics>> GetPipelineAnalytics(Guid pipelineId);
    }

    public record DealAnalytics(
        int TotalDeals,
        int ActiveDeals,
        int CompletedDeals,
        decimal TotalDealAmount,
        decimal AverageDealAmount,
        TimeSpan AverageDealDuration
    );

    public record DealStageAnalytics(
        Guid StageId,
        string StageName,
        int DealCount,
        TimeSpan AverageTimeInStage,
        int OverdueDeals
    );
}
