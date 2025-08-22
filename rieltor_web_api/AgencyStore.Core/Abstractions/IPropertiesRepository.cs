using AgencyStore.Core.Models;

namespace PropertyStore.DataAccess.Repository
{
    public interface IPropertiesRepository
    {
        Task<Guid> Create(Property property);
        Task<Guid> Delete(Guid id);
        Task<List<Property>> Get();
        Task<Guid> Update(Guid id, string title, string type, decimal price, string address,
    decimal area, int rooms, string description, bool isActive, DateTime createdAt);

        Task<Property?> GetById(Guid id); // Для получения конкретного property
        Task AddImageToProperty(Guid propertyId, PropertyImage image);
        Task RemoveImageFromProperty(Guid propertyId, Guid imageId);
        Task SetMainImage(Guid propertyId, Guid imageId);
    }
}