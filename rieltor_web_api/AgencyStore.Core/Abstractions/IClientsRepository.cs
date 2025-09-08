using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IClientsRepository
    {
        Task<Guid> Create(Client client);
        Task<Guid> Delete(Guid id);
        Task<List<Client>> Get();
        Task<Client?> GetById(Guid id);
        Task<Guid> Update(Guid id, string name, string phone, string? email,
                         string source, string? notes, DateTime createdAt);

        // Для поиска клиента по телефону (чтобы избежать дубликатов)
        Task<Client?> GetByPhone(string phone);

        // Для получения клиента с его заявками
        Task<Client?> GetWithRequests(Guid id);
    }
}
