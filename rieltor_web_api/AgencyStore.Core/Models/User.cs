using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyStore.Core.Models
{
    public class User
    {
        public const int MIN_PASSWORD_LENGTH = 6;

        private User(int id, string username, string passwordHash, string role, DateTime createdAt)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = createdAt;
        }

        public int Id { get; }
        public string Username { get; } = string.Empty;
        public string PasswordHash { get; } = string.Empty;
        public string Role { get; } = "Admin";
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public static (User user, string Error) Create(int id, string username, string passwordHash, string role)
        {
            var error = string.Empty;

            if (string.IsNullOrEmpty(username))
                error = "Username cannot be empty";
            else if (string.IsNullOrEmpty(passwordHash)) 
                error = "Password hash cannot be empty";
            else if (string.IsNullOrEmpty(role))
                error = "Role cannot be empty";

            var user = new User(id, username, passwordHash, role, DateTime.UtcNow); 
            return (user, error);
        }
    }
}
