using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class RequestsRepository : IRequestsRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public RequestsRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }
        public async Task<Guid> Create(Request request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var requestEntity = new RequestEntity
                {
                    Id = request.Id,
                    ClientId = request.ClientId,
                    PropertyId = request.PropertyId,
                    Type = request.Type,
                    Status = request.Status,
                    Message = request.Message,
                    CreatedAt = request.CreatedAt
                };

                await _dbContext.Requests.AddAsync(requestEntity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return request.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _dbContext.Requests
                .Where(r => r.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

        public async Task<List<Request>> Get()
        {
            var requestEntities = await _dbContext.Requests
                .Include(r => r.Client)
                .Include(r => r.Property)
                .AsNoTracking()
                .ToListAsync();

            return requestEntities.Select(MapToDomain).ToList();
        }

        public async Task<List<Request>> GetByClientId(Guid clientId)
        {
            var requestEntities = await _dbContext.Requests
                .Include(r => r.Client)
                .Include(r => r.Property)
                .Where(r => r.ClientId == clientId)
                .AsNoTracking()
                .ToListAsync();

            return requestEntities.Select(MapToDomain).ToList();
        }

        public async Task<Request?> GetById(Guid id)
        {
            var requestEntity = await _dbContext.Requests
                .Include(r => r.Client)
                .Include(r => r.Property)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            return requestEntity != null ? MapToDomain(requestEntity) : null;
        }

        public async Task<List<Request>> GetByStatus(string status)
        {
            var requestEntities = await _dbContext.Requests
                .Include(r => r.Client)
                .Include(r => r.Property)
                .Where(r => r.Status == status)
                .AsNoTracking()
                .ToListAsync();

            return requestEntities.Select(MapToDomain).ToList();
        }

        public async Task<Request?> GetWithDetails(Guid id)
        {
            var requestEntity = await _dbContext.Requests
                .Include(r => r.Client)
                .Include(r => r.Property)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            return requestEntity != null ? MapToDomain(requestEntity) : null;
        }

        private Request MapToDomain(RequestEntity entity)
        {
            var (request, error) = Request.Create(
                entity.Id,
                entity.ClientId,
                entity.PropertyId,
                entity.Type,
                entity.Status,
                entity.Message,
                entity.CreatedAt
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid request data: {error}");

            return request;
        }

        public async Task<Guid> UpdateStatus(Guid id, string status)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.Requests
                    .Where(r => r.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.Status, r => status));

                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
