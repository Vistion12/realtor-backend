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
            var response = properties.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PropertiesResponse>> GetPropertyById(Guid id)
        {
            var property = await _propertiesService.GetPropertyById(id);
            if (property == null)
                return NotFound();

            return Ok(MapToResponse(property));
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
                DateTime.UtcNow 
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            var propertyId = await _propertiesService.CreateProperty(property);
            return Ok(propertyId);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateProperties(Guid id, [FromBody] PropertiesRequest request)
        {
            var propertyId = await _propertiesService.UpdateProperty(
                id, request.Title, request.Type, request.Price,
                request.Address, request.Area, request.Rooms, request.Description,  // ← Добавьте request.Area
                request.IsActive, DateTime.UtcNow);
            return Ok(propertyId);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteProperty(Guid id)
        {
            return Ok(await _propertiesService.DeleteProperty(id));
        }

        [HttpPost("{propertyId:guid}/images")]
        public async Task<ActionResult> AddImageToProperty(Guid propertyId, [FromBody] PropertyImageRequest request)
        {
            try
            {
                await _propertiesService.AddImageToProperty(propertyId, request.Url, request.IsMain);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{propertyId:guid}/images/{imageId:guid}")]
        public async Task<ActionResult> RemoveImageFromProperty(Guid propertyId, Guid imageId)
        {
            try
            {
                await _propertiesService.RemoveImageFromProperty(propertyId, imageId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{propertyId:guid}/images/{imageId:guid}/main")]
        public async Task<ActionResult> SetMainImage(Guid propertyId, Guid imageId)
        {
            try
            {
                await _propertiesService.SetMainImage(propertyId, imageId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Вспомогательный метод для маппинга
        private PropertiesResponse MapToResponse(Property property)
        {
            return new PropertiesResponse(
                property.Id,
                property.Title,
                property.Type,
                property.Price,
                property.Address,
                property.Area,
                property.Rooms,
                property.Description,
                property.IsActive,
                property.CreatedAt,
                property.Images.Select(i => new PropertyImageResponse(i.Id, i.Url, i.IsMain, i.Order)).ToList()
            );
        }

    }
}
