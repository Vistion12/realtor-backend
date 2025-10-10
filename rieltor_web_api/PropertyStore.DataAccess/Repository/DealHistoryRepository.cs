

using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class DealHistoryRepository : IDealHistoryRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public DealHistoryRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Create(DealHistory history)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new DealHistoryEntity
                {
                    Id = history.Id,
                    DealId = history.DealId,
                    FromStageId = history.FromStageId,
                    ToStageId = history.ToStageId,
                    Notes = history.Notes,
                    ChangedAt = history.ChangedAt,
                    TimeInStage = history.TimeInStage
                };

                await _dbContext.DealHistory.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return history.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TimeSpan> GetAverageTimeInStage(Guid stageId)
        {
            var averageSeconds = await _dbContext.DealHistory
                .Where(h => h.FromStageId == stageId && h.TimeInStage > TimeSpan.Zero)
                .AverageAsync(h => (double?)h.TimeInStage.TotalSeconds) ?? 0;

            return TimeSpan.FromSeconds(averageSeconds);
        }

        public async Task<List<DealHistory>> GetByDealId(Guid dealId)
        {
            var entities = await _dbContext.DealHistory
                .Include(h => h.FromStage)
                .Include(h => h.ToStage)
                .Where(h => h.DealId == dealId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<DealHistory>> GetByStageId(Guid stageId)
        {
            var entities = await _dbContext.DealHistory
                .Include(h => h.FromStage)
                .Include(h => h.ToStage)
                .Include(h => h.Deal)
                .Where(h => h.FromStageId == stageId || h.ToStageId == stageId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<DealHistory>> GetRecentHistory(int count = 10)
        {
            var entities = await _dbContext.DealHistory
                .Include(h => h.FromStage)
                .Include(h => h.ToStage)
                .Include(h => h.Deal)
                .OrderByDescending(h => h.ChangedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        private DealHistory MapToDomain(DealHistoryEntity entity)
        {
            var history = DealHistory.Create(
                entity.Id,
                entity.DealId,
                entity.FromStageId,
                entity.ToStageId,
                entity.TimeInStage,
                entity.Notes
            );

            history.ChangedAt = entity.ChangedAt;

            // Маппинг этапов если они загружены
            if (entity.FromStage != null)
            {
                var (fromStage, fromError) = DealStage.Create(
                    entity.FromStage.Id,
                    entity.FromStage.Name,
                    entity.FromStage.Order,
                    entity.FromStage.ExpectedDuration,
                    entity.FromStage.PipelineId,
                    entity.FromStage.Description
                );

                if (string.IsNullOrEmpty(fromError))
                {
                    fromStage.CreatedAt = entity.FromStage.CreatedAt;
                    history.FromStage = fromStage;
                }
            }

            if (entity.ToStage != null)
            {
                var (toStage, toError) = DealStage.Create(
                    entity.ToStage.Id,
                    entity.ToStage.Name,
                    entity.ToStage.Order,
                    entity.ToStage.ExpectedDuration,
                    entity.ToStage.PipelineId,
                    entity.ToStage.Description
                );

                if (string.IsNullOrEmpty(toError))
                {
                    toStage.CreatedAt = entity.ToStage.CreatedAt;
                    history.ToStage = toStage;
                }
            }

            return history;
        }
    }
}
