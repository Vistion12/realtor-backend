using AgencyStore.Core.Models;
using PropertyStore.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<Guid> UpdateProperty(Guid id, string title, string type, decimal price, string addres,
            int rooms, string description, bool isActive, DateTime createdAt)
        {
            return await _propertiesRepository.Update(id, title, type, price, addres, rooms, description, isActive, createdAt);
        }

        public async Task<Guid> DeleteProperty(Guid id)
        {
            return await _propertiesRepository.Delete(id);

        }
    }
}
