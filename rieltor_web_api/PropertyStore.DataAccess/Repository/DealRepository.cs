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
    public class DealRepository : IDealRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public DealRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Create(Deal deal)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new DealEntity
                {
                    Id = deal.Id,
                    Title = deal.Title,
                    Notes = deal.Notes,
                    DealAmount = deal.DealAmount,
                    ExpectedCloseDate = deal.ExpectedCloseDate,
                    CurrentStageId = deal.CurrentStageId,
                    PipelineId = deal.PipelineId,
                    ClientId = deal.ClientId,
                    PropertyId = deal.PropertyId,
                    RequestId = deal.RequestId,
                    StageStartedAt = deal.StageStartedAt,
                    StageDeadline = deal.StageDeadline,
                    CreatedAt = deal.CreatedAt,
                    UpdatedAt = deal.UpdatedAt,
                    ClosedAt = deal.ClosedAt,
                    IsActive = deal.IsActive
                };

                await _dbContext.Deals.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return deal.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _dbContext.Deals
                .Where(d => d.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

        public async Task<List<Deal>> Get()
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Include(d => d.Property)
                .Include(d => d.Request)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<Deal>> GetActiveDeals()
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<Deal>> GetByClientId(Guid clientId)
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.ClientId == clientId)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<Deal>> GetByPipelineId(Guid pipelineId)
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.PipelineId == pipelineId)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<Deal>> GetByStageId(Guid stageId)
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.CurrentStageId == stageId && d.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<Deal?> GetById(Guid id)
        {
            var entity = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Include(d => d.Property)
                .Include(d => d.Request)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            return entity != null ? MapToDomain(entity) : null;
        }

        public async Task<Deal?> GetByIdWithDetails(Guid id)
        {
            return await GetById(id);
            //var result = await _dbContext.Deals
            //    .Where(d => d.Id == id)
            //    .Select(d => new
            //    {
            //        Deal = d,
            //        Client = d.Client,
            //        Pipeline = d.Pipeline,
            //        CurrentStage = d.CurrentStage,
            //        Property = d.Property,
            //        Request = d.Request,
            //        History = d.History.Select(h => new
            //        {
            //            History = h,
            //            FromStage = h.FromStage,
            //            ToStage = h.ToStage
            //        }).OrderBy(h => h.History.ChangedAt).ToList()
            //    })
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync();

            //if (result == null) return null;

            //return MapToDomainWithProjection(result);
        }

        private Deal MapToDomainWithProjection(dynamic result)
        {
            var entity = (DealEntity)result.Deal;
            var clientEntity = (ClientEntity?)result.Client;
            var pipelineEntity = (DealPipelineEntity?)result.Pipeline;
            var currentStageEntity = (DealStageEntity?)result.CurrentStage;
            var propertyEntity = (PropertyEntity?)result.Property;
            var requestEntity = (RequestEntity?)result.Request;
            var historyData = (List<dynamic>?)result.History;

            var (deal, error) = Deal.Create(
                entity.Id,
                entity.Title,
                entity.ClientId,
                entity.PipelineId,
                entity.CurrentStageId,
                entity.PropertyId,
                entity.RequestId,
                entity.Notes,
                entity.DealAmount,
                entity.ExpectedCloseDate
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid deal data: {error}");

            deal.StageStartedAt = entity.StageStartedAt;
            deal.StageDeadline = entity.StageDeadline;
            deal.CreatedAt = entity.CreatedAt;
            deal.UpdatedAt = entity.UpdatedAt;
            deal.ClosedAt = entity.ClosedAt;
            deal.IsActive = entity.IsActive;

            // Маппинг связанных сущностей из projection
            if (clientEntity != null)
            {
                var (client, clientError) = Client.Create(
                    clientEntity.Id,
                    clientEntity.Name,
                    clientEntity.Phone,
                    clientEntity.Email,
                    clientEntity.Source,
                    clientEntity.Notes,
                    clientEntity.CreatedAt
                );
                if (string.IsNullOrEmpty(clientError)) deal.Client = client;
            }

            if (pipelineEntity != null)
            {
                var (pipeline, pipelineError) = DealPipeline.Create(
                    pipelineEntity.Id,
                    pipelineEntity.Name,
                    pipelineEntity.Description
                );
                if (string.IsNullOrEmpty(pipelineError))
                {
                    pipeline.IsActive = pipelineEntity.IsActive;
                    pipeline.CreatedAt = pipelineEntity.CreatedAt;
                    pipeline.UpdatedAt = pipelineEntity.UpdatedAt;
                    deal.Pipeline = pipeline;
                }
            }

            if (currentStageEntity != null)
            {
                var (stage, stageError) = DealStage.Create(
                    currentStageEntity.Id,
                    currentStageEntity.Name,
                    currentStageEntity.Order,
                    currentStageEntity.ExpectedDuration,
                    currentStageEntity.PipelineId,
                    currentStageEntity.Description
                );
                if (string.IsNullOrEmpty(stageError))
                {
                    stage.CreatedAt = currentStageEntity.CreatedAt;
                    deal.CurrentStage = stage;
                }
            }

            // Маппинг истории из projection
            if (historyData != null)
            {
                foreach (var historyItem in historyData)
                {
                    var historyEntity = (DealHistoryEntity)historyItem.History;
                    var fromStageEntity = (DealStageEntity)historyItem.FromStage;
                    var toStageEntity = (DealStageEntity)historyItem.ToStage;

                    var history = DealHistory.Create(
                        historyEntity.Id,
                        historyEntity.DealId,
                        historyEntity.FromStageId,
                        historyEntity.ToStageId,
                        historyEntity.TimeInStage,
                        historyEntity.Notes
                    );

                    history.ChangedAt = historyEntity.ChangedAt;

                    // Маппинг этапов истории
                    if (fromStageEntity != null)
                    {
                        var (fromStage, fromError) = DealStage.Create(
                            fromStageEntity.Id,
                            fromStageEntity.Name,
                            fromStageEntity.Order,
                            fromStageEntity.ExpectedDuration,
                            fromStageEntity.PipelineId,
                            fromStageEntity.Description
                        );
                        if (string.IsNullOrEmpty(fromError))
                        {
                            fromStage.CreatedAt = fromStageEntity.CreatedAt;
                            history.FromStage = fromStage;
                        }
                    }

                    if (toStageEntity != null)
                    {
                        var (toStage, toError) = DealStage.Create(
                            toStageEntity.Id,
                            toStageEntity.Name,
                            toStageEntity.Order,
                            toStageEntity.ExpectedDuration,
                            toStageEntity.PipelineId,
                            toStageEntity.Description
                        );
                        if (string.IsNullOrEmpty(toError))
                        {
                            toStage.CreatedAt = toStageEntity.CreatedAt;
                            history.ToStage = toStage;
                        }
                    }

                    deal.History.Add(history);
                }
            }

            return deal;
        }

        public async Task<int> GetDealsCountByStage(Guid stageId)
        {
            return await _dbContext.Deals
                .CountAsync(d => d.CurrentStageId == stageId && d.IsActive);
        }

        public async Task<List<Deal>> GetDealsByStatus(bool isActive)
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.IsActive == isActive)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<Deal>> GetOverdueDeals()
        {
            var entities = await _dbContext.Deals
                .Include(d => d.Client)
                .Include(d => d.Pipeline)
                .Include(d => d.CurrentStage)
                .Where(d => d.IsActive && d.StageDeadline.HasValue && d.StageDeadline.Value < DateTime.UtcNow)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<Guid> Update(Deal deal)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.Deals
                    .Where(d => d.Id == deal.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(d => d.Title, d => deal.Title)
                        .SetProperty(d => d.Notes, d => deal.Notes)
                        .SetProperty(d => d.DealAmount, d => deal.DealAmount)
                        .SetProperty(d => d.ExpectedCloseDate, d => deal.ExpectedCloseDate)
                        .SetProperty(d => d.CurrentStageId, d => deal.CurrentStageId)
                        .SetProperty(d => d.StageStartedAt, d => deal.StageStartedAt)
                        .SetProperty(d => d.StageDeadline, d => deal.StageDeadline)
                        .SetProperty(d => d.UpdatedAt, d => DateTime.UtcNow)
                        .SetProperty(d => d.ClosedAt, d => deal.ClosedAt)
                        .SetProperty(d => d.IsActive, d => deal.IsActive));

                await transaction.CommitAsync();
                return deal.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private Deal MapToDomain(DealEntity entity)
        {
            var (deal, error) = Deal.Create(
                entity.Id,
                entity.Title,
                entity.ClientId,
                entity.PipelineId,
                entity.CurrentStageId,
                entity.PropertyId,
                entity.RequestId,
                entity.Notes,
                entity.DealAmount,
                entity.ExpectedCloseDate
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid deal data: {error}");

            deal.StageStartedAt = entity.StageStartedAt;
            deal.StageDeadline = entity.StageDeadline;
            deal.CreatedAt = entity.CreatedAt;
            deal.UpdatedAt = entity.UpdatedAt;
            deal.ClosedAt = entity.ClosedAt;
            deal.IsActive = entity.IsActive;

            // Маппинг связанных сущностей
            if (entity.Client != null)
            {
                var (client, clientError) = Client.Create(
                    entity.Client.Id,
                    entity.Client.Name,
                    entity.Client.Phone,
                    entity.Client.Email,
                    entity.Client.Source,
                    entity.Client.Notes,
                    entity.Client.CreatedAt
                );

                if (string.IsNullOrEmpty(clientError))
                {
                    deal.Client = client;
                }
            }

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
                    deal.Pipeline = pipeline;
                }
            }

            if (entity.CurrentStage != null)
            {
                var (stage, stageError) = DealStage.Create(
                    entity.CurrentStage.Id,
                    entity.CurrentStage.Name,
                    entity.CurrentStage.Order,
                    entity.CurrentStage.ExpectedDuration,
                    entity.CurrentStage.PipelineId,
                    entity.CurrentStage.Description
                );

                if (string.IsNullOrEmpty(stageError))
                {
                    stage.CreatedAt = entity.CurrentStage.CreatedAt;
                    deal.CurrentStage = stage;
                }
            }

            // Маппинг истории
            foreach (var historyEntity in entity.History.OrderBy(h => h.ChangedAt))
            {
                var history = DealHistory.Create(
                    historyEntity.Id,
                    historyEntity.DealId,
                    historyEntity.FromStageId,
                    historyEntity.ToStageId,
                    historyEntity.TimeInStage,
                    historyEntity.Notes
                );

                history.ChangedAt = historyEntity.ChangedAt;
                deal.History.Add(history);
            }

            return deal;
        }
    }
}
