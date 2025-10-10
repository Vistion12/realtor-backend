using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Repository
{
    public class DealStageRepository : IDealStageRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public DealStageRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Create(DealStage stage)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new DealStageEntity
                {
                    Id = stage.Id,
                    Name = stage.Name,
                    Description = stage.Description,
                    Order = stage.Order,
                    ExpectedDuration = stage.ExpectedDuration,
                    PipelineId = stage.PipelineId,
                    CreatedAt = stage.CreatedAt
                };

                await _dbContext.DealStages.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return stage.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _dbContext.DealStages
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

        public async Task<List<DealStage>> Get()
        {
            var entities = await _dbContext.DealStages
                .Include(s => s.Pipeline)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<DealStage?> GetById(Guid id)
        {
            var entity = await _dbContext.DealStages
                .Include(s => s.Pipeline)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<List<DealStage>> GetByPipelineId(Guid pipelineId)
        {
            var entities = await _dbContext.DealStages
                .Include(s => s.Pipeline)
                .Where(s => s.PipelineId == pipelineId)
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<DealStage?> GetNextStage(Guid currentStageId)
        {
            var currentStage = await _dbContext.DealStages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == currentStageId);

            if (currentStage == null)
                return null;

            var nextStage = await _dbContext.DealStages
                .Where(s => s.PipelineId == currentStage.PipelineId && s.Order > currentStage.Order)
                .OrderBy(s => s.Order)
                .FirstOrDefaultAsync();

            return nextStage != null ? MapToDomain(nextStage) : null;
        }

        public async Task<DealStage?> GetPreviousStage(Guid currentStageId)
        {
            var currentStage = await _dbContext.DealStages
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == currentStageId);

            if (currentStage == null)
                return null;

            var previousStage = await _dbContext.DealStages
                .Where(s => s.PipelineId == currentStage.PipelineId && s.Order < currentStage.Order)
                .OrderByDescending(s => s.Order)
                .FirstOrDefaultAsync();

            return previousStage != null ? MapToDomain(previousStage) : null;
        }

        public async Task<List<DealStage>> GetStagesWithDeals(Guid pipelineId)
        {
            var entities = await _dbContext.DealStages
                .Include(s => s.Pipeline)
                .Include(s => s.Deals.Where(d => d.IsActive))
                .Where(s => s.PipelineId == pipelineId)
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<Guid> Update(DealStage stage)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.DealStages
                    .Where(s => s.Id == stage.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(st => st.Name, st => stage.Name)
                        .SetProperty(st => st.Description, st => stage.Description)
                        .SetProperty(st => st.Order, st => stage.Order)
                        .SetProperty(st => st.ExpectedDuration, st => stage.ExpectedDuration));

                await transaction.CommitAsync();
                return stage.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private DealStage MapToDomain(DealStageEntity entity)
        {
            var (stage, error) = DealStage.Create(
                entity.Id,
                entity.Name,
                entity.Order,
                entity.ExpectedDuration,
                entity.PipelineId,
                entity.Description
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid stage data: {error}");

            stage.CreatedAt = entity.CreatedAt;

            // Маппинг Pipeline если он загружен
            if (entity.Pipeline != null)
            {
                var (pipeline, pipelineError) = DealPipeline.Create(
                    entity.Pipeline.Id,
                    entity.Pipeline.Name,
                    entity.Pipeline.Description
                );

                if (string.IsNullOrEmpty(pipelineError))
                {
                    pipeline.IsActive = entity.Pipeline.IsActive;
                    pipeline.CreatedAt = entity.Pipeline.CreatedAt;
                    pipeline.UpdatedAt = entity.Pipeline.UpdatedAt;
                    stage.Pipeline = pipeline;
                }
            }

            return stage;
        }
    }
}
