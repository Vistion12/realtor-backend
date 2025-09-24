using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly PropertyStoreDBContext _context;

        public UserRepository(PropertyStoreDBContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            return userEntity == null ? null : MapToDomain(userEntity);
        }

        public async Task<User> CreateAsync(User user)
        {
            var userEntity = new UserEntity
            {
                Username = user.Username,
                PasswordHash = user.PasswordHash, 
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return MapToDomain(userEntity);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        private User MapToDomain(UserEntity entity)
        {
            var (user, error) = User.Create(
                entity.Id,
                entity.Username,
                entity.PasswordHash, 
                entity.Role
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid user data: {error}");

            return user;
        }
    }
}
