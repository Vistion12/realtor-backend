using AgencyStore.Core.Models;

namespace PropertyStore.Application.Services
{
    public interface IPropertiesService
    {
        Task<Guid> CreateProperty(Property property);
        Task<Guid> DeleteProperty(Guid id);
        Task<List<Property>> GetAllProperties();
        Task<Guid> UpdateProperty(Guid id, string title, string type, decimal price, string address,
    decimal area, int rooms, string description, bool isActive, DateTime createdAt);

        Task<Property?> GetPropertyById(Guid id);
        Task AddImageToProperty(Guid propertyId, string imageUrl, bool isMain = false);
        Task RemoveImageFromProperty(Guid propertyId, Guid imageId);
        Task SetMainImage(Guid propertyId, Guid imageId);
    }
}