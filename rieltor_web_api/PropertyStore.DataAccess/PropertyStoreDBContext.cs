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
        public DbSet<ClientEntity> Clients { get; set; }
        public DbSet<RequestEntity> Requests { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<DealPipelineEntity> DealPipelines { get; set; }
        public DbSet<DealStageEntity> DealStages { get; set; }
        public DbSet<DealEntity> Deals { get; set; }
        public DbSet<DealHistoryEntity> DealHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PropertyConfiguration());
            modelBuilder.ApplyConfiguration(new PropertyImageConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new RequestConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new DealPipelineConfiguration());
            modelBuilder.ApplyConfiguration(new DealStageConfiguration());
            modelBuilder.ApplyConfiguration(new DealConfiguration());
            modelBuilder.ApplyConfiguration(new DealHistoryConfiguration());
        }
    }
}
