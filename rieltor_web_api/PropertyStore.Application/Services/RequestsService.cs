using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;

namespace PropertyStore.Application.Services
{
    public class RequestsService : IRequestsService
    {
        private readonly IRequestsRepository _requestsRepository;
        private readonly IClientsService _clientsService;

        public RequestsService(IRequestsRepository requestsRepository, IClientsService clientsService)
        {
            _requestsRepository = requestsRepository;
            _clientsService = clientsService;
        }
        public async Task<Guid> CreateRequest(Request request)
        {
            return await _requestsRepository.Create(request);
        }

        public async Task<Guid> CreateRequestWithClient(Guid? propertyId, string type, string message, string clientName, string clientPhone, string? clientEmail, string source)
        {
            // Находим или создаем клиента
            var client = await _clientsService.FindOrCreateClient(clientName, clientPhone, clientEmail, source);

            // Создаем заявку
            var requestId = Guid.NewGuid();
            var (request, error) = Request.Create(
                requestId,
                client.Id,
                propertyId,
                type,
                "new", // статус по умолчанию
                message,
                DateTime.UtcNow
            );

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return await _requestsRepository.Create(request);
        }

        public async Task<Guid> DeleteRequest(Guid id)
        {
            return await _requestsRepository.Delete(id);
        }

        public async Task<List<Request>> GetAllRequests()
        {
            return await _requestsRepository.Get();
        }

        public async Task<Request?> GetRequestById(Guid id)
        {
            return await _requestsRepository.GetById(id);
        }

        public async Task<List<Request>> GetRequestsByClientId(Guid clientId)
        {
            return await _requestsRepository.GetByClientId(clientId);
        }

        public async Task<List<Request>> GetRequestsByStatus(string status)
        {
            return await _requestsRepository.GetByStatus(status);
        }

        public async Task<Request?> GetRequestWithDetails(Guid id)
        {
            return await _requestsRepository.GetWithDetails(id);
        }

        public  async Task<Guid> UpdateRequestStatus(Guid id, string status)
        {
            return await _requestsRepository.UpdateStatus(id, status);
        }
    }
}
