using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IRequestsRepository
    {
        Task<Guid> Create(Request request);
        Task<Guid> Delete(Guid id);
        Task<List<Request>> Get();
        Task<Request?> GetById(Guid id);

        // Для получения заявок по статусу
        Task<List<Request>> GetByStatus(string status);

        // Для получения заявок клиента
        Task<List<Request>> GetByClientId(Guid clientId);

        // Для обновления статуса заявки
        Task<Guid> UpdateStatus(Guid id, string status);

        // Для получения заявки с клиентом и объектом недвижимости
        Task<Request?> GetWithDetails(Guid id);

    }
}
