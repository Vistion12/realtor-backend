using AgencyStore.Core.Models;
using PropertyStore.DataAccess.Repository;

namespace PropertyStore.Application.Services
{
    public class PropertiesService : IPropertiesService
    {
        private readonly IPropertiesRepository _propertiesRepository;

        public PropertiesService(IPropertiesRepository propertiesRepository)
        {
            _propertiesRepository = propertiesRepository;
        }

        public async Task<List<Property>> GetAllProperties()
        {
            return await _propertiesRepository.Get();
        }

        public async Task<Guid> CreateProperty(Property property)
        {
            return await _propertiesRepository.Create(property);
        }

        public async Task<Guid> UpdateProperty(Guid id, string title, string type, decimal price, string address,
    decimal area, int rooms, string description, bool isActive, DateTime createdAt)
        {
            return await _propertiesRepository.Update(id, title, type, price, address,
                area, rooms, description, isActive, createdAt);
        }

        public async Task RemoveAllImagesFromProperty(Guid propertyId)
        {
            await _propertiesRepository.RemoveAllImagesFromProperty(propertyId);
        }

        public async Task AddImagesToProperty(Guid propertyId, List<PropertyImage> images)
        {
            foreach (var image in images)
            {
                if (image.PropertyId != propertyId)
                    throw new ArgumentException("Image does not belong to this property");

                var (_, error) = PropertyImage.Create(image.Id, image.PropertyId, image.Url, image.IsMain);

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);
            }

            // Здесь нужно добавить реализацию массового добавления в репозиторий
            // Или вызывать AddImageToProperty для каждого изображения
            foreach (var image in images)
            {
                await _propertiesRepository.AddImageToProperty(propertyId, image);
            }
        }

        public async Task<Guid> DeleteProperty(Guid id)
        {
            return await _propertiesRepository.Delete(id);
        }

        public async Task<Property?> GetPropertyById(Guid id)
        {
            return await _propertiesRepository.GetById(id);
        }

        public async Task AddImageToProperty(Guid propertyId, string imageUrl, bool isMain = false)
        {
            var imageId = Guid.NewGuid();

            var (image, error) = PropertyImage.Create(imageId, propertyId, imageUrl, isMain);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            await _propertiesRepository.AddImageToProperty(propertyId, image);
        }

        public async Task RemoveImageFromProperty(Guid propertyId, Guid imageId)
        {
            await _propertiesRepository.RemoveImageFromProperty(propertyId, imageId);
        }

        public async Task SetMainImage(Guid propertyId, Guid imageId)
        {
            await _propertiesRepository.SetMainImage(propertyId, imageId);
        }
    }
}
