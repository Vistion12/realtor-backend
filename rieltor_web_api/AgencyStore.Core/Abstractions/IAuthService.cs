

namespace AgencyStore.Core.Abstractions
{
    public interface IAuthService
    {
        Task<(string Token, string Username, string Role, DateTime Expires)?> AuthenticateAsync(string username, string password);
        Task<bool> UserExistsAsync(string username);
    }
}
