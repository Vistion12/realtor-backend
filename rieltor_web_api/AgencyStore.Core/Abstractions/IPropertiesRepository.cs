using AgencyStore.Core.Models;

namespace PropertyStore.DataAccess.Repository
{
    public interface IPropertiesRepository
    {
        Task<Guid> Create(Property property);
        Task<Guid> Delete(Guid id);
        Task<List<Property>> Get();
        Task<Guid> Update(Guid id, string title, string type, decimal price, string addres, 
            int rooms, string description, bool isActive, DateTime createdAt);
    }
}