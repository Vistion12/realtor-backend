using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class ClientsRepository : IClientsRepository
    {
        private readonly PropertyStoreDBContext _dbContext;
        private readonly ILogger<ClientsRepository> _logger;

        public ClientsRepository(PropertyStoreDBContext context, ILogger<ClientsRepository> logger)
        {
            _dbContext = context;
            _logger = logger;
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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Сначала удаляем связанные данные вручную
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"ClientDocuments\" WHERE \"ClientId\" = {0}", id);

                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"Deals\" WHERE \"ClientId\" = {0}", id);

                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"Requests\" WHERE \"ClientId\" = {0}", id);

                // 2. Удаляем файлы
                await DeleteClientFiles(id);

                // 3. Теперь удаляем клиента
                var client = await _dbContext.Clients.FindAsync(id);
                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task DeleteClientFiles(Guid clientId)
        {
            try
            {
                var clientFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "clients", clientId.ToString());
                if (Directory.Exists(clientFolder))
                {
                    Directory.Delete(clientFolder, recursive: true);
                    _logger.LogInformation("Файлы клиента {ClientId} удалены", clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления файлов клиента {ClientId}", clientId);
            }
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

            // УСТАНАВЛИВАЕМ ПОЛЯ ЛК!
            client.SetClientAccountInfo(
                hasPersonalAccount: entity.HasPersonalAccount,
                accountLogin: entity.AccountLogin,
                temporaryPassword: entity.TemporaryPassword,
                isAccountActive: entity.IsAccountActive,
                consentToPersonalData: entity.ConsentToPersonalData,
                consentGivenAt: entity.ConsentGivenAt,
                consentIpAddress: entity.ConsentIpAddress
            );

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

        public async Task<Client?> GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var clientEntity = await _dbContext.Clients
               .Include(c => c.Requests)
               .AsNoTracking()
               .FirstOrDefaultAsync(c => c.Email == email);

            return clientEntity != null ? MapToDomain(clientEntity) : null;
        }

        public async Task<Client?> GetClientWithDocuments(Guid id)
        {
            var entity = await _dbContext.Clients
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.Id == id);

            return entity == null ? null : MapToDomain(entity);
        }

        public async Task<List<Client>> GetClientsWithPersonalAccounts()
        {
            var entities = await _dbContext.Clients
                .Where(c => c.HasPersonalAccount)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<bool> UpdateClientAccountInfo(Client client)
        {
            var entity = await _dbContext.Clients.FindAsync(client.Id);
            if (entity == null)
                return false;

            // Обновляем только поля связанные с аккаунтом
            entity.HasPersonalAccount = client.HasPersonalAccount;
            entity.AccountLogin = client.AccountLogin;
            entity.TemporaryPassword = client.TemporaryPassword;
            entity.IsAccountActive = client.IsAccountActive;
            entity.ConsentToPersonalData = client.ConsentToPersonalData;
            entity.ConsentGivenAt = client.ConsentGivenAt;
            entity.ConsentIpAddress = client.ConsentIpAddress;

            _dbContext.Clients.Update(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Client?> GetByAccountLogin(string login)
        {
            var entity = await _dbContext.Clients
                .FirstOrDefaultAsync(c => c.AccountLogin == login && c.IsAccountActive);

            return entity == null ? null : MapToDomain(entity);
        }
    }
}
