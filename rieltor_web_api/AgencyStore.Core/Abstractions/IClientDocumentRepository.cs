using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IClientDocumentRepository
    {
        Task<Guid> Create(ClientDocument document);
        Task<ClientDocument?> GetById(Guid id);
        Task<List<ClientDocument>> GetByClientId(Guid clientId);
        Task<List<ClientDocument>> GetByClientIdAndCategory(Guid clientId, string category);
        Task<List<ClientDocument>> GetRequiredDocuments(Guid clientId);
        Task<List<ClientDocument>> GetOverdueDocuments();
        Task<Guid> Delete(Guid id);
        Task<bool> DocumentExists(Guid id);
    }
}
