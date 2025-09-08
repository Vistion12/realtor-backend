using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IClientsService
    {
        Task<Guid> CreateClient(Client client);
        Task<Guid> DeleteClient(Guid id);
        Task<List<Client>> GetAllClients();
        Task<Client?> GetClientById(Guid id);
        Task<Guid> UpdateClient(Guid id, string name, string phone, string? email,
                               string source, string? notes, DateTime createdAt);

        // Поиск или создание клиента по телефону (для заявок)
        Task<Client> FindOrCreateClient(string name, string phone, string? email, string source);

        // Получение клиента с его заявками
        Task<Client?> GetClientWithRequests(Guid id);
    }
}
