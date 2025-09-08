using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class ClientsRepository : IClientsRepository
    {
        private readonly PropertyStoreDBContext _dbContext;

        public ClientsRepository(PropertyStoreDBContext context)
        {
            _dbContext = context;
        }

        public async Task<Guid> Create(Client client)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var clientEntity = new ClientEntity
                {
                    Id = client.Id,
                    Name = client.Name,
                    Phone = client.Phone,
                    Email = client.Email,
                    Source = client.Source,
                    Notes = client.Notes,
                    CreatedAt = client.CreatedAt
                };

                await _dbContext.Clients.AddAsync(clientEntity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return client.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _dbContext.Clients
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

        public async Task<List<Client>> Get()
        {
            var clientEntities = await _dbContext.Clients
                .Include(c => c.Requests)
                .AsNoTracking()
                .ToListAsync();

            return clientEntities.Select(MapToDomain).ToList();
        }

        public async Task<Client?> GetById(Guid id)
        {
            var clientEntity = await _dbContext.Clients
                .Include(c => c.Requests)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return clientEntity != null ? MapToDomain(clientEntity) : null;
        }

        public async Task<Client?> GetByPhone(string phone)
        {
            var clientEntity = await _dbContext.Clients
               .Include(c => c.Requests)
               .AsNoTracking()
               .FirstOrDefaultAsync(c => c.Phone == phone);

            return clientEntity != null ? MapToDomain(clientEntity) : null;
        }

        public async Task<Client?> GetWithRequests(Guid id)
        {
            var clientEntity = await _dbContext.Clients
                .Include(c => c.Requests)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            return clientEntity != null ? MapToDomain(clientEntity) : null;
        }

        private Client MapToDomain(ClientEntity entity)
        {
            var (client, error) = Client.Create(
                entity.Id,
                entity.Name,
                entity.Phone,
                entity.Email,
                entity.Source,
                entity.Notes,
                entity.CreatedAt
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid client data: {error}");

            // Добавляем заявки клиента
            foreach (var requestEntity in entity.Requests)
            {
                var (request, requestError) = Request.Create(
                    requestEntity.Id,
                    requestEntity.ClientId,
                    requestEntity.PropertyId,
                    requestEntity.Type,
                    requestEntity.Status,
                    requestEntity.Message,
                    requestEntity.CreatedAt
                );

                if (string.IsNullOrEmpty(requestError))
                {
                    client.AddRequest(request);
                }
            }

            return client;
        }

        public async Task<Guid> Update(Guid id, string name, string phone, string? email, string source, string? notes, DateTime createdAt)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _dbContext.Clients
                    .Where(c => c.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.Name, c => name)
                        .SetProperty(c => c.Phone, c => phone)
                        .SetProperty(c => c.Email, c => email)
                        .SetProperty(c => c.Source, c => source)
                        .SetProperty(c => c.Notes, c => notes)
                        .SetProperty(c => c.CreatedAt, c => createdAt));

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
