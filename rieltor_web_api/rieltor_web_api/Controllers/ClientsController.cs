using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rieltor_web_api.Contracts;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;
        private readonly IRequestsService _requestsService;

        public ClientsController(IClientsService clientsService, IRequestsService requestsService)
        {
            _clientsService = clientsService;
            _requestsService = requestsService;
        }


        [HttpGet]
        public async Task<ActionResult<List<ClientResponse>>> GetClients()
        {
            var clients = await _clientsService.GetAllClients();
            var response = clients.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientResponse>> GetClientById(Guid id)
        {
            var client = await _clientsService.GetClientById(id);
            if (client == null)
                return NotFound();

            return Ok(MapToResponse(client));
        }

        [HttpGet("{id:guid}/requests")]
        public async Task<ActionResult<List<RequestResponse>>> GetClientRequests(Guid id)
        {
            var requests = await _requestsService.GetRequestsByClientId(id);
            var response = requests.Select(MapToRequestResponse);
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateClient([FromBody] ClientRequest request)
        {
            var (client, error) = Client.Create(
                Guid.NewGuid(),
                request.Name,
                request.Phone,
                request.Email,
                request.Source,
                request.Notes,
                DateTime.UtcNow
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            try
            {
                var clientId = await _clientsService.CreateClient(client);
                return Ok(clientId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании клиента: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateClient(Guid id, [FromBody] ClientRequest request)
        {
            try
            {
                var clientId = await _clientsService.UpdateClient(
                    id, request.Name, request.Phone, request.Email,
                    request.Source, request.Notes, DateTime.UtcNow);

                return Ok(clientId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении клиента: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteClient(Guid id)
        {
            try
            {
                return Ok(await _clientsService.DeleteClient(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении клиента: {ex.Message}");
            }
        }

        private ClientResponse MapToResponse(Client client)
        {
            return new ClientResponse(
                client.Id,
                client.Name,
                client.Phone,
                client.Email,
                client.Source,
                client.Notes,
                client.CreatedAt
            );
        }

        private RequestResponse MapToRequestResponse(Request request)
        {
            return new RequestResponse(
                request.Id,
                request.ClientId,
                request.PropertyId,
                request.Type,
                request.Status,
                request.Message,
                request.CreatedAt,
                null  // не включаем клиента в список заявок
            );
        }
    }
}
