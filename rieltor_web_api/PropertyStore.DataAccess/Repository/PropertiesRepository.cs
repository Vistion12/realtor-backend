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
            var propertyEntities = await dbContext.Properties
                .Include(p => p.Images)
                .AsNoTracking()
                .ToListAsync();

            return propertyEntities.Select(MapToDomain).ToList();
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
                Area = property.Area,
                Rooms = property.Rooms,
                Description = property.Description,
                IsActive = property.IsActive,
                CreatedAt = property.CreatedAt,

            };
            await dbContext.Properties.AddAsync(propertyEntiry);
            await dbContext.SaveChangesAsync();

            return property.Id;
        }

        public async Task<Guid> Update(Guid id, string title, string type, decimal price, string address, decimal area,
                                int rooms, string description, bool isActive, DateTime createdAt)
        {
            await dbContext.Properties
               .Where(p => p.Id == id)
               .ExecuteUpdateAsync(s => s
                   .SetProperty(p => p.Title, p => title)
                   .SetProperty(p => p.Type, p => type)
                   .SetProperty(p => p.Price, p => price)
                   .SetProperty(p => p.Address, p => address)
                   .SetProperty(p => p.Area, p => area)          // ← ДОБАВЬТЕ ЭТУ СТРОКУ!
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

        public async Task<Property?> GetById(Guid id)
        {
            var propertyEntity = await dbContext.Properties
                .Include(p => p.Images) // Важно: включаем изображения
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (propertyEntity == null)
                return null;

            return MapToDomain(propertyEntity);
        }

        public async Task AddImageToProperty(Guid propertyId, PropertyImage image)
        {
            // Создаем imageEntity без предварительной загрузки PropertyEntity
            var imageEntity = new PropertyImageEntity
            {
                Id = image.Id,
                PropertyId = propertyId, // Просто устанавливаем foreign key
                Url = image.Url,
                IsMain = image.IsMain,
                Order = image.Order
            };

            // Добавляем напрямую в DbSet
            await dbContext.PropertyImages.AddAsync(imageEntity);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveImageFromProperty(Guid propertyId, Guid imageId)
        {
            await dbContext.PropertyImages
                .Where(i => i.Id == imageId && i.PropertyId == propertyId)
                .ExecuteDeleteAsync();
        }

        public async Task SetMainImage(Guid propertyId, Guid imageId)
        {
            // Проверяем существование изображения
            var imageExists = await dbContext.PropertyImages
                .AnyAsync(i => i.Id == imageId && i.PropertyId == propertyId);

            if (!imageExists)
                throw new ArgumentException("Image not found");

            // Сбрасываем все флаги isMain
            await dbContext.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.IsMain, i => false));

            // Устанавливаем новый main image
            await dbContext.PropertyImages
                .Where(i => i.Id == imageId && i.PropertyId == propertyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.IsMain, i => true));
        }
        // Вспомогательный метод для маппинга
        private Property MapToDomain(PropertyEntity entity)
        {
            var (property, error) = Property.Create(
                entity.Id,
                entity.Title,
                entity.Type,
                entity.Price,
                entity.Address,
                entity.Area,
                entity.Rooms,
                entity.Description,
                entity.IsActive,
                entity.CreatedAt
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid property data: {error}");

            // Добавляем изображения
            foreach (var imageEntity in entity.Images.OrderBy(i => i.Order))
            {
                var (image, imageError) = PropertyImage.Create(
                    imageEntity.Id,
                    imageEntity.PropertyId,
                    imageEntity.Url,
                    imageEntity.IsMain,
                    imageEntity.Order
                );

                if (string.IsNullOrEmpty(imageError))
                    property.AddImage(image);
            }

            return property;
        }
    }
}
