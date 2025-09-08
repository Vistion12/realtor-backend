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
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Создаем основную сущность
                var propertyEntity = new PropertyEntity
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

                await dbContext.Properties.AddAsync(propertyEntity);

                // Добавляем изображения
                if (property.Images.Any())
                {
                    var imageEntities = property.Images.Select((image, index) => new PropertyImageEntity
                    {
                        Id = image.Id,
                        PropertyId = property.Id,
                        Url = image.Url,
                        IsMain = image.IsMain,
                        Order = index
                    }).ToList();

                    await dbContext.PropertyImages.AddRangeAsync(imageEntities);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return property.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> Update(Guid id, string title, string type, decimal price, string address,
                            decimal area, int rooms, string description, bool isActive, DateTime createdAt)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Обновляем основные данные
                await dbContext.Properties
                   .Where(p => p.Id == id)
                   .ExecuteUpdateAsync(s => s
                       .SetProperty(p => p.Title, p => title)
                       .SetProperty(p => p.Type, p => type)
                       .SetProperty(p => p.Price, p => price)
                       .SetProperty(p => p.Address, p => address)
                       .SetProperty(p => p.Area, p => area)
                       .SetProperty(p => p.Rooms, p => rooms)
                       .SetProperty(p => p.Description, p => description)
                       .SetProperty(p => p.IsActive, p => isActive)
                       .SetProperty(p => p.CreatedAt, p => createdAt));

                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveAllImagesFromProperty(Guid propertyId)
        {
            await dbContext.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .ExecuteDeleteAsync();
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
            if (image.IsMain)
            {
                await dbContext.PropertyImages
                    .Where(i => i.PropertyId == propertyId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(i => i.IsMain, i => false));
            }

            var maxOrder = await dbContext.PropertyImages
                .Where(i => i.PropertyId == propertyId)
                .MaxAsync(i => (int?)i.Order) ?? -1;

            var imageEntity = new PropertyImageEntity
            {
                Id = image.Id,
                PropertyId = propertyId,
                Url = image.Url,
                IsMain = image.IsMain,
                Order = maxOrder + 1  
            };

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

            // Добавляем изображения в правильном порядке
            foreach (var imageEntity in entity.Images.OrderBy(i => i.Order))
            {
                var (image, imageError) = PropertyImage.Create(
                    imageEntity.Id,
                    imageEntity.PropertyId,
                    imageEntity.Url,
                    imageEntity.IsMain
                );

                if (string.IsNullOrEmpty(imageError))
                {
                    var imageWithOrder = new
                    {
                        Image = image,
                        Order = imageEntity.Order
                    };
                    
                    property.AddImage(image);
                }
            }

            return property;
        }
    }
}
