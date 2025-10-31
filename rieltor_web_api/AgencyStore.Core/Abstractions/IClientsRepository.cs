using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IClientsRepository
    {
        Task<List<Client>> Get();
        Task<Client?> GetById(Guid id);        
        Task<Client?> GetByPhone(string phone); // Для поиска клиента по телефону (чтобы избежать дубликатов)
        Task<Client?> GetByEmail(string email);
        Task<Client?> GetWithRequests(Guid id); // Для получения клиента с его заявками
        Task<Guid> Create(Client client);
        Task<Guid> Update(Guid id, string name, string phone, string? email,
                         string source, string? notes, DateTime createdAt);   
        Task<Guid> Delete(Guid id);

        Task<Client?> GetClientWithDocuments(Guid id);
        Task<List<Client>> GetClientsWithPersonalAccounts();
        Task<bool> UpdateClientAccountInfo(Client client);
        Task<Client?> GetByAccountLogin(string login);
    }
}
