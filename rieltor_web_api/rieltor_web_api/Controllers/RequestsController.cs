using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Mvc;
using PropertyStore.Application.Services;
using rieltor_web_api.Contracts;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestsService _requestsService;
        private readonly IClientsService _clientsService;

        public RequestsController(IRequestsService requestsService, IClientsService clientsService)
        {
            _requestsService = requestsService;
            _clientsService = clientsService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RequestResponse>>> GetRequests()
        {
            var requests = await _requestsService.GetAllRequests();
            var response = requests.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RequestResponse>> GetRequestById(Guid id)
        {
            var request = await _requestsService.GetRequestById(id);
            if (request == null)
                return NotFound();

            return Ok(MapToResponse(request));
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<RequestResponse>>> GetRequestsByStatus(string status)
        {
            var requests = await _requestsService.GetRequestsByStatus(status);
            var response = requests.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("client/{clientId:guid}")]
        public async Task<ActionResult<List<RequestResponse>>> GetRequestsByClientId(Guid clientId)
        {
            var requests = await _requestsService.GetRequestsByClientId(clientId);
            var response = requests.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}/with-details")]
        public async Task<ActionResult<RequestResponse>> GetRequestWithDetails(Guid id)
        {
            var request = await _requestsService.GetRequestById(id);
            if (request == null)
                return NotFound();

            // Получаем клиента для детализированного ответа
            var client = await _clientsService.GetClientById(request.ClientId);
            var clientResponse = client != null ? MapToClientResponse(client) : null;

            return Ok(MapToDetailedResponse(request, clientResponse));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateRequest([FromBody] RequestRequest request)
        {
            try
            {
                var requestId = await _requestsService.CreateRequestWithClient(
                    request.PropertyId,
                    request.Type,
                    request.Message,
                    request.ClientName,
                    request.ClientPhone,
                    request.ClientEmail,
                    request.Source
                );

                return Ok(requestId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании заявки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<Guid>> UpdateRequestStatus(Guid id, [FromBody] string status)
        {
            try
            {
                var requestId = await _requestsService.UpdateRequestStatus(id, status);
                return Ok(requestId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении статуса: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteRequest(Guid id)
        {
            try
            {
                return Ok(await _requestsService.DeleteRequest(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении заявки: {ex.Message}");
            }
        }

        private RequestResponse MapToResponse(Request request)
        {
            return new RequestResponse(
                request.Id,
                request.ClientId,
                request.PropertyId,
                request.Type,
                request.Status,
                request.Message,
                request.CreatedAt,
                null  // клиент не включается в базовый ответ
            );
        }

        private RequestResponse MapToDetailedResponse(Request request, ClientResponse? clientResponse)
        {
            return new RequestResponse(
                request.Id,
                request.ClientId,
                request.PropertyId,
                request.Type,
                request.Status,
                request.Message,
                request.CreatedAt,
                clientResponse  // включаем клиента в детализированный ответ
            );
        }

        private ClientResponse MapToClientResponse(Client client)
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
    }
}