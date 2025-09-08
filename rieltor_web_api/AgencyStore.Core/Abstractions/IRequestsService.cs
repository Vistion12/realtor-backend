using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IRequestsService
    {
        Task<Guid> CreateRequest(Request request);
        Task<Guid> DeleteRequest(Guid id);
        Task<List<Request>> GetAllRequests();
        Task<Request?> GetRequestById(Guid id);
        Task<Guid> UpdateRequestStatus(Guid id, string status);

        // Создание заявки с автоматическим созданием/поиском клиента
        Task<Guid> CreateRequestWithClient(Guid? propertyId, string type, string message,
                                         string clientName, string clientPhone,
                                         string? clientEmail, string source);

        // Получение заявок по статусу
        Task<List<Request>> GetRequestsByStatus(string status);

        // Получение заявок клиента
        Task<List<Request>> GetRequestsByClientId(Guid clientId);

        // Получение заявки с деталями
        Task<Request?> GetRequestWithDetails(Guid id);
    }
}
