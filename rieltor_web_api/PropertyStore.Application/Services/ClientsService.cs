using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;

namespace PropertyStore.Application.Services
{
    public class ClientsService : IClientsService
    {

        private readonly IClientsRepository _clientsRepository;

        public ClientsService(IClientsRepository clientsRepository)
        {
            _clientsRepository = clientsRepository;
        }

        public async Task<Guid> CreateClient(Client client)
        {
            return await _clientsRepository.Create(client);
        }

        public async Task<Guid> DeleteClient(Guid id)
        {
            return await _clientsRepository.Delete(id);
        }

        public async Task<Client> FindOrCreateClient(string name, string phone, string? email, string source)
        {
            // Ищем существующего клиента по телефону
            var existingClient = await _clientsRepository.GetByPhone(phone);
            if (existingClient != null)
            {
                return existingClient;
            }

            // Создаем нового клиента
            var newClientId = Guid.NewGuid();
            var (client, error) = Client.Create(
                newClientId,
                name,
                phone,
                email,
                source,
                null, // notes
                DateTime.UtcNow
            );

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            await _clientsRepository.Create(client);
            return client;
        }

        public async Task<List<Client>> GetAllClients()
        {
            return await _clientsRepository.Get();
        }

        public async Task<Client?> GetClientById(Guid id)
        {
            return await _clientsRepository.GetById(id);
        }

        public async Task<Client?> GetClientWithRequests(Guid id)
        {
            return await _clientsRepository.GetWithRequests(id);
        }

        public async Task<Guid> UpdateClient(Guid id, string name, string phone, string? email, string source, string? notes, DateTime createdAt)
        {
            return await _clientsRepository.Update(id, name, phone, email, source, notes, createdAt);
        }
    }
}
