using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Configuration;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess
{
    public class PropertyStoreDBContext : DbContext
    {
        public PropertyStoreDBContext(DbContextOptions<PropertyStoreDBContext> options) : base(options)  
        {
        }
        public DbSet<PropertyEntity> Properties { get; set; }
        public DbSet<PropertyImageEntity> PropertyImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PropertyConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyImageConfiguration());
        }
    }
}
