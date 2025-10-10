using AgencyStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyStore.Core.Abstractions
{
    public interface IDealPipelineService
    {
        Task<Guid> CreatePipeline(DealPipeline pipeline);
        Task<Guid> UpdatePipeline(DealPipeline pipeline);
        Task<Guid> DeletePipeline(Guid id);
        Task<List<DealPipeline>> GetAllPipelines();
        Task<DealPipeline?> GetPipelineById(Guid id);
        Task<DealPipeline?> GetPipelineWithStages(Guid id);
        Task<List<DealPipeline>> GetActivePipelines();
        Task<bool> InitializeDefaultPipelines();
    }
}
