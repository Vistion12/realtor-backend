

using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealStageRepository
    {
        Task<Guid> Create(DealStage stage);
        Task<Guid> Update(DealStage stage);
        Task<Guid> Delete(Guid id);
        Task<List<DealStage>> Get();
        Task<DealStage?> GetById(Guid id);
        Task<List<DealStage>> GetByPipelineId(Guid pipelineId);
        Task<DealStage?> GetNextStage(Guid currentStageId);
        Task<DealStage?> GetPreviousStage(Guid currentStageId);
        Task<List<DealStage>> GetStagesWithDeals(Guid pipelineId);
    }
}
