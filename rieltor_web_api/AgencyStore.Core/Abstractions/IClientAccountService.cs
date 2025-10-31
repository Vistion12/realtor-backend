using AgencyStore.Core.Models;


namespace AgencyStore.Core.Abstractions
{
    public interface IClientAccountService
    {
        Task<(bool success, string error)> ActivateClientAccount(Guid clientId, string temporaryPassword);
        Task<(bool success, string error)> GiveConsent(Guid clientId, string ipAddress);
        Task<(bool success, string error)> ChangeClientPassword(Guid clientId, string newPassword);
        Task<(bool success, string error)> DeactivateClientAccount(Guid clientId);
        Task<Client?> GetClientByAccountLogin(string login);
    }
}
