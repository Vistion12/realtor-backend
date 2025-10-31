using AgencyStore.Core.Models;

namespace AgencyStore.Core.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<bool> UserExistsAsync(string username);
    }
}
