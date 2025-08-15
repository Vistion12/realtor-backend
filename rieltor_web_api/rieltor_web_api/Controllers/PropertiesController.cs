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
        
    }
}
