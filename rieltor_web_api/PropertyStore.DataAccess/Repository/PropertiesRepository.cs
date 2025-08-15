using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class PropertiesRepository : IPropertiesRepository
    {
        private readonly PropertyStoreDBContext dbContext;
        public PropertiesRepository(PropertyStoreDBContext context)
        {
            dbContext = context;
        }

        public async Task<List<Property>> Get()
        {
            var propertyEntities = await dbContext.Properties.AsNoTracking().ToListAsync();

            var properties = propertyEntities
                .Select(p => Property.Create(p.Id, p.Title, p.Type, p.Price, p.Address, p.Area, p.Rooms,
                                            p.Description, p.IsActive, p.CreatedAt).property).ToList();
            return properties;
        }

        public async Task<Guid> Create(Property property)
        {
            var propertyEntiry = new PropertyEntity
            {
                Id = property.Id,
                Title = property.Title,
                Type = property.Type,
                Price = property.Price,
                Address = property.Address,
                Rooms = property.Rooms,
                Description = property.Description,
                IsActive = property.IsActive,
                CreatedAt = property.CreatedAt,

            };
            await dbContext.Properties.AddAsync(propertyEntiry);
            await dbContext.SaveChangesAsync();

            return property.Id;
        }

        public async Task<Guid> Update(Guid id, string title, string type, decimal price, string addres,
                                        int rooms, string description, bool isActive, DateTime createdAt)
        {
            await dbContext.Properties
               .Where(p => p.Id == id)
               .ExecuteUpdateAsync(s => s
                   .SetProperty(p => p.Title, p => title)
                   .SetProperty(p => p.Type, p => type)
                   .SetProperty(p => p.Price, p => price)
                   .SetProperty(p => p.Address, p => addres)
                   .SetProperty(p => p.Rooms, p => rooms)
                   .SetProperty(p => p.Description, p => description)
                   .SetProperty(p => p.IsActive, p => isActive)
                   .SetProperty(p => p.CreatedAt, p => createdAt));

            return id;
        }

        public async Task<Guid> Delete(Guid id)
        {
            await dbContext.Properties
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();
            return id;
        }

    }
}
