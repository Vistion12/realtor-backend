using AgencyStore.Core.Models;


namespace AgencyStore.Core.Abstractions
{
    public interface IDealPipelineRepository
    {
        Task<Guid> Create(DealPipeline pipeline);
        Task<Guid> Update(DealPipeline pipeline);
        Task<Guid> Delete(Guid id);
        Task<List<DealPipeline>> Get();
        Task<DealPipeline?> GetById(Guid id);
        Task<DealPipeline?> GetByIdWithStages(Guid id);
        Task<List<DealPipeline>> GetActivePipelines();
        Task<DealPipeline?> GetByName(string name);
    }
}
