using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess
{
    public class PropertyStoreDBContext : DbContext
    {
        public PropertyStoreDBContext(DbContextOptions<PropertyStoreDBContext> options) : base(options)  
        {
        }
        public DbSet<PropertyEntity> Properties { get; set; }
    }
}
