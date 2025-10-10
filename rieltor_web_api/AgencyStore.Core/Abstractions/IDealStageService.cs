
using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealStageService
    {
        Task<Guid> CreateStage(DealStage stage);
        Task<Guid> UpdateStage(DealStage stage);
        Task<Guid> DeleteStage(Guid id);
        Task<List<DealStage>> GetAllStages();
        Task<DealStage?> GetStageById(Guid id);
        Task<List<DealStage>> GetStagesByPipeline(Guid pipelineId);
        Task<DealStage?> GetNextStage(Guid currentStageId);
        Task<DealStage?> GetPreviousStage(Guid currentStageId);
        Task ReorderStages(Guid pipelineId, List<Guid> stageIdsInOrder);
    }
}
