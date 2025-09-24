using AgencyStore.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyStore.Core.Abstractions
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<bool> UserExistsAsync(string username);
    }
}
