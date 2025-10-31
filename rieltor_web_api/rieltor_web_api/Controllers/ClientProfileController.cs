using AgencyStore.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rieltor_web_api.Contracts;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/client/profile")]
    [Authorize(Roles = "Client")]
    public class ClientProfileController : ControllerBase
    {
        private readonly IClientsService _clientsService;
        private readonly IDealService _dealService;

        public ClientProfileController(IClientsService clientsService, IDealService dealService)
        {
            _clientsService = clientsService;
            _dealService = dealService;
        }

        [HttpGet]
        public async Task<ActionResult<ClientResponse>> GetProfile()
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                var client = await _clientsService.GetClientById(clientId);
                if (client == null)
                    return NotFound("Клиент не найден");

                return Ok(MapToResponse(client));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения профиля: {ex.Message}");
            }
        }

        [HttpGet("deals")]
        public async Task<ActionResult<List<ClientDealResponse>>> GetClientDeals()
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                // ИСПОЛЬЗУЕМ СУЩЕСТВУЮЩИЙ МЕТОД ИЗ СЕРВИСА
                var clientDeals = await _dealService.GetDealsByClient(clientId);

                // МАППИМ В СПЕЦИАЛЬНЫЙ DTO ДЛЯ КЛИЕНТА
                var response = clientDeals.Select(MapToClientDealResponse).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения сделок: {ex.Message}");
            }
        }

        [HttpGet("deals/{dealId:guid}")]
        public async Task<ActionResult<ClientDealDetailsResponse>> GetDealDetails(Guid dealId)
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                var deal = await _dealService.GetDealWithDetails(dealId);
                if (deal == null || deal.ClientId != clientId)
                    return NotFound("Сделка не найдена");

                // МАППИМ В ДЕТАЛЬНЫЙ DTO ДЛЯ КЛИЕНТА
                var response = MapToClientDealDetailsResponse(deal);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения сделки: {ex.Message}");
            }
        }

        private Guid GetClientIdFromToken()
        {
            var clientIdClaim = User.FindFirst("ClientId");
            if (clientIdClaim == null || !Guid.TryParse(clientIdClaim.Value, out var clientId))
                return Guid.Empty;

            return clientId;
        }

        private ClientResponse MapToResponse(AgencyStore.Core.Models.Client client)
        {
            return new ClientResponse(
                client.Id,
                client.Name,
                client.Phone,
                client.Email,
                client.Source,
                client.Notes,
                client.CreatedAt,
                client.HasPersonalAccount,
                client.AccountLogin,
                client.IsAccountActive,
                client.ConsentToPersonalData
            );
        }

        private ClientDealResponse MapToClientDealResponse(AgencyStore.Core.Models.Deal deal)
        {
            var status = GetDealStatus(deal);

            return new ClientDealResponse(
                deal.Id,
                deal.Title,
                deal.Notes,
                deal.DealAmount,
                deal.ExpectedCloseDate,
                deal.PipelineId,
                deal.Pipeline?.Name ?? "Неизвестная воронка",
                deal.CurrentStageId,
                deal.CurrentStage?.Name ?? "Неизвестный этап",
                deal.StageStartedAt,
                deal.StageDeadline,
                deal.CreatedAt,
                deal.IsActive,
                deal.IsOverdue(),
                status
            );
        }

        private ClientDealDetailsResponse MapToClientDealDetailsResponse(AgencyStore.Core.Models.Deal deal)
        {
            var currentStage = new CurrentStageInfo(
                deal.CurrentStage?.Id ?? Guid.Empty,
                deal.CurrentStage?.Name ?? "Неизвестный этап",
                deal.CurrentStage?.Description,
                deal.CurrentStage?.Order ?? 0
            );

            var history = deal.History?.Select(h => new DealHistoryEntry(
                h.Id,
                h.FromStage?.Name ?? "Неизвестный этап", // ИСПРАВЛЕНО: FromStage вместо PreviousStage
                h.ToStage?.Name ?? "Неизвестный этап",   // ИСПРАВЛЕНО: ToStage вместо NewStage
                h.TimeInStage,                           // ИСПРАВЛЕНО: TimeInStage вместо TimeInPreviousStage
                h.ChangedAt,                             // ИСПРАВЛЕНО: ChangedAt вместо CreatedAt
                h.Notes
            )).ToList() ?? new List<DealHistoryEntry>();

            var status = GetDealStatus(deal);

            return new ClientDealDetailsResponse(
                deal.Id,
                deal.Title,
                deal.Notes,
                deal.DealAmount,
                deal.ExpectedCloseDate,
                deal.Pipeline?.Name ?? "Неизвестная воронка",
                currentStage,
                deal.StageStartedAt,
                deal.StageDeadline,
                deal.CreatedAt,
                deal.IsActive,
                deal.IsOverdue(),
                history
            );
        }

        private string GetDealStatus(AgencyStore.Core.Models.Deal deal)
        {
            if (!deal.IsActive) return "completed";
            if (deal.IsOverdue()) return "overdue";
            return "active";
        }
    }
}