using AgencyStore.Core.Models;

namespace PropertyStore.Application.Services
{
    public interface IPropertiesService
    {
        Task<Guid> CreateProperty(Property property);
        Task<Guid> DeleteProperty(Guid id);
        Task<List<Property>> GetAllProperties();
        Task<Guid> UpdateProperty(Guid id, string title, string type, decimal price, string addres, int rooms, string description, bool isActive, DateTime createdAt);
    }
}