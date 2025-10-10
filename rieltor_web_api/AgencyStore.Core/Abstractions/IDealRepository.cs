

using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealRepository
    {
        Task<Guid> Create(Deal deal);
        Task<Guid> Update(Deal deal);
        Task<Guid> Delete(Guid id);
        Task<List<Deal>> Get();
        Task<Deal?> GetById(Guid id);
        Task<Deal?> GetByIdWithDetails(Guid id);
        Task<List<Deal>> GetByClientId(Guid clientId);
        Task<List<Deal>> GetByPipelineId(Guid pipelineId);
        Task<List<Deal>> GetByStageId(Guid stageId);
        Task<List<Deal>> GetActiveDeals();
        Task<List<Deal>> GetOverdueDeals();
        Task<List<Deal>> GetDealsByStatus(bool isActive);
        Task<int> GetDealsCountByStage(Guid stageId);
    }
}
