using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.Application.Services
{
    public class DealPipelineService : IDealPipelineService
    {
        private readonly IDealPipelineRepository _pipelineRepository;
        private readonly IDealStageRepository _stageRepository;

        public DealPipelineService(
            IDealPipelineRepository pipelineRepository,
            IDealStageRepository stageRepository)
        {
            _pipelineRepository = pipelineRepository;
            _stageRepository = stageRepository;
        }

        public async Task<Guid> CreatePipeline(DealPipeline pipeline)
        {
            // Проверяем уникальность названия
            var existing = await _pipelineRepository.GetByName(pipeline.Name);
            if (existing != null)
                throw new ArgumentException($"Воронка с названием '{pipeline.Name}' уже существует");

            return await _pipelineRepository.Create(pipeline);
        }

        public async Task<Guid> DeletePipeline(Guid id)
        {
            // Проверяем, нет ли активных сделок в этой воронке
            var pipeline = await _pipelineRepository.GetByIdWithStages(id);
            if (pipeline == null)
                throw new ArgumentException("Воронка не найдена");

            // Можно добавить проверку на наличие активных сделок

            return await _pipelineRepository.Delete(id);
        }

        public async Task<List<DealPipeline>> GetAllPipelines()
        {
            return await _pipelineRepository.Get();
        }

        public async Task<List<DealPipeline>> GetActivePipelines()
        {
            return await _pipelineRepository.GetActivePipelines();
        }

        public async Task<DealPipeline?> GetPipelineById(Guid id)
        {
            return await _pipelineRepository.GetById(id);
        }

        public async Task<DealPipeline?> GetPipelineWithStages(Guid id)
        {
            return await _pipelineRepository.GetByIdWithStages(id);
        }

        public async Task<Guid> UpdatePipeline(DealPipeline pipeline)
        {
            var existing = await _pipelineRepository.GetById(pipeline.Id);
            if (existing == null)
                throw new ArgumentException("Воронка не найдена");

            // Проверяем уникальность названия
            var duplicate = await _pipelineRepository.GetByName(pipeline.Name);
            if (duplicate != null && duplicate.Id != pipeline.Id)
                throw new ArgumentException($"Воронка с названием '{pipeline.Name}' уже существует");

            return await _pipelineRepository.Update(pipeline);
        }

        public async Task<bool> InitializeDefaultPipelines()
        {
            try
            {
                // Создаем стандартные воронки если их нет
                var defaultPipelines = new[]
                {
                    new { Name = "Покупка недвижимости", Description = "Процесс покупки недвижимости для клиента" },
                    new { Name = "Продажа недвижимости", Description = "Процесс продажи недвижимости клиента" },
                    new { Name = "Аренда", Description = "Процесс аренды недвижимости" }
                };

                foreach (var pipelineInfo in defaultPipelines)
                {
                    var existing = await _pipelineRepository.GetByName(pipelineInfo.Name);
                    if (existing == null)
                    {
                        var (pipeline, error) = DealPipeline.Create(
                            Guid.NewGuid(),
                            pipelineInfo.Name,
                            pipelineInfo.Description
                        );

                        if (string.IsNullOrEmpty(error))
                        {
                            await _pipelineRepository.Create(pipeline);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
