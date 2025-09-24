using PropertyStore.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Seed
{
    public static class UserSeeder
    {
        public static void Seed(PropertyStoreDBContext context)
        {
            
            if (!context.Users.Any())
            {
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "admin123";

                context.Users.Add(new UserEntity
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                });

                context.SaveChanges();
                Console.WriteLine("Admin user seeded successfully");
            }
            else
            {
                Console.WriteLine("Users already exist, skipping seed");
            }
        }
    }
}
