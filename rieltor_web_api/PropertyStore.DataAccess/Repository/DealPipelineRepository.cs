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
    public class DealPipelineRepository : IDealPipelineRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public DealPipelineRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Create(DealPipeline pipeline)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new DealPipelineEntity
                {
                    Id = pipeline.Id,
                    Name = pipeline.Name,
                    Description = pipeline.Description,
                    IsActive = pipeline.IsActive,
                    CreatedAt = pipeline.CreatedAt,
                    UpdatedAt = pipeline.UpdatedAt
                };

                await _dbContext.DealPipelines.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return pipeline.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _dbContext.DealPipelines
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

        public async Task<List<DealPipeline>> Get()
        {
            var entities = await _dbContext.DealPipelines
                .Include(p => p.Stages)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<DealPipeline>> GetActivePipelines()
        {
            var entities = await _dbContext.DealPipelines
                .Include(p => p.Stages)
                .Where(p => p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<DealPipeline?> GetById(Guid id)
        {
            var entity = await _dbContext.DealPipelines
                .Include(p => p.Stages)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<DealPipeline?> GetByIdWithStages(Guid id)
        {
            var entity = await _dbContext.DealPipelines
                .Include(p => p.Stages.OrderBy(s => s.Order))
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<DealPipeline?> GetByName(string name)
        {
            var entity = await _dbContext.DealPipelines
                .Include(p => p.Stages)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == name);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<Guid> Update(DealPipeline pipeline)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.DealPipelines
                    .Where(p => p.Id == pipeline.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Name, p => pipeline.Name)
                        .SetProperty(p => p.Description, p => pipeline.Description)
                        .SetProperty(p => p.IsActive, p => pipeline.IsActive)
                        .SetProperty(p => p.UpdatedAt, p => DateTime.UtcNow));

                await transaction.CommitAsync();
                return pipeline.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private DealPipeline MapToDomain(DealPipelineEntity entity)
        {
            var (pipeline, error) = DealPipeline.Create(
                entity.Id,
                entity.Name,
                entity.Description
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid pipeline data: {error}");

            pipeline.IsActive = entity.IsActive;
            pipeline.CreatedAt = entity.CreatedAt;
            pipeline.UpdatedAt = entity.UpdatedAt;

            // Добавляем этапы
            foreach (var stageEntity in entity.Stages.OrderBy(s => s.Order))
            {
                var (stage, stageError) = DealStage.Create(
                    stageEntity.Id,
                    stageEntity.Name,
                    stageEntity.Order,
                    stageEntity.ExpectedDuration,
                    stageEntity.PipelineId,
                    stageEntity.Description
                );

                if (string.IsNullOrEmpty(stageError))
                {
                    stage.CreatedAt = stageEntity.CreatedAt;
                    pipeline.Stages.Add(stage);
                }
            }

            return pipeline;
        }
    }
}
