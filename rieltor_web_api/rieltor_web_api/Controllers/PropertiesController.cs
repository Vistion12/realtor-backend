using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Mvc;
using PropertyStore.Application.Services;
using rieltor_web_api.Contracts;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertiesService _propertiesService;

        public PropertiesController(IPropertiesService propertiesService)
        {
            _propertiesService = propertiesService;
        }

        [HttpGet]
        public async Task <ActionResult<List<PropertiesResponse>>> GetProperties()
        {
            var properties = await _propertiesService.GetAllProperties();

            var response = properties.Select(p => new PropertiesResponse(p.Id, p.Title, p.Type, p.Price, p.Address, p.Area,
                                                                         p.Rooms, p.Description, p.IsActive, p.CreatedAt));
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateProperty([FromBody] PropertiesRequest request)
        {
            var (property, error) = Property.Create(
                Guid.NewGuid(),
                request.Title,
                request.Type,
                request.Price,
                request.Address,
                request.Area,
                request.Rooms,
                request.Description,
                request.IsActive,
                request.CreatedAt
                );

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(error);
            }

            var propertyId = await _propertiesService.CreateProperty(property);

            return Ok(propertyId);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProperties(Guid id, [FromBody] PropertiesRequest request)
        {
            var propertyId = await _propertiesService.UpdateProperty(id, request.Title, request.Type, 
                request.Price, request.Address,request.Rooms,request.Description,request.IsActive,request.CreatedAt);
            return Ok(propertyId);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteProperty(Guid id)
        {
            return Ok(await _propertiesService.DeleteProperty(id));
        }
        
    }
}
