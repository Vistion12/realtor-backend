using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.Application.Services
{
    public class DealStageService : IDealStageService
    {
        private readonly IDealStageRepository _stageRepository;
        private readonly IDealPipelineRepository _pipelineRepository;

        public DealStageService(
            IDealStageRepository stageRepository,
            IDealPipelineRepository pipelineRepository)
        {
            _stageRepository = stageRepository;
            _pipelineRepository = pipelineRepository;
        }

        public async Task<Guid> CreateStage(DealStage stage)
        {
            // Проверяем существование воронки
            var pipeline = await _pipelineRepository.GetById(stage.PipelineId);
            if (pipeline == null)
                throw new ArgumentException("Воронка не найдена");

            // Проверяем уникальность порядка в рамках воронки
            var stages = await _stageRepository.GetByPipelineId(stage.PipelineId);
            if (stages.Any(s => s.Order == stage.Order))
                throw new ArgumentException($"Этап с порядком {stage.Order} уже существует в этой воронке");

            return await _stageRepository.Create(stage);
        }

        public async Task<Guid> DeleteStage(Guid id)
        {
            // Можно добавить проверку на использование этапа в активных сделках
            return await _stageRepository.Delete(id);
        }

        public async Task<List<DealStage>> GetAllStages()
        {
            return await _stageRepository.Get();
        }

        public async Task<DealStage?> GetNextStage(Guid currentStageId)
        {
            return await _stageRepository.GetNextStage(currentStageId);
        }

        public async Task<DealStage?> GetPreviousStage(Guid currentStageId)
        {
            return await _stageRepository.GetPreviousStage(currentStageId);
        }

        public async Task<DealStage?> GetStageById(Guid id)
        {
            return await _stageRepository.GetById(id);
        }

        public async Task<List<DealStage>> GetStagesByPipeline(Guid pipelineId)
        {
            return await _stageRepository.GetByPipelineId(pipelineId);
        }

        public async Task<Guid> UpdateStage(DealStage stage)
        {
            var existing = await _stageRepository.GetById(stage.Id);
            if (existing == null)
                throw new ArgumentException("Этап не найден");

            // Проверяем уникальность порядка
            var stages = await _stageRepository.GetByPipelineId(stage.PipelineId);
            if (stages.Any(s => s.Order == stage.Order && s.Id != stage.Id))
                throw new ArgumentException($"Этап с порядком {stage.Order} уже существует в этой воронке");

            return await _stageRepository.Update(stage);
        }

        public async Task ReorderStages(Guid pipelineId, List<Guid> stageIdsInOrder)
        {
            var stages = await _stageRepository.GetByPipelineId(pipelineId);

            for (int i = 0; i < stageIdsInOrder.Count; i++)
            {
                var stageId = stageIdsInOrder[i];
                var stage = stages.FirstOrDefault(s => s.Id == stageId);
                if (stage != null && stage.Order != i)
                {
                    stage.Order = i;
                    await _stageRepository.Update(stage);
                }
            }
        }
    }
}
