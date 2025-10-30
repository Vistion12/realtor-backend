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
    public class DealsController : ControllerBase
    {
        private readonly IDealService _dealService;
        private readonly IClientsService _clientsService;
        private readonly IDealPipelineService _pipelineService;
        private readonly IDealStageService _stageService;
        private readonly ILogger<DealsController> _logger;

        public DealsController(
            IDealService dealService,
            IClientsService clientsService,
            IDealPipelineService pipelineService,
            IDealStageService stageService,
            ILogger<DealsController> logger)
        {
            _dealService = dealService;
            _clientsService = clientsService;
            _pipelineService = pipelineService;
            _stageService = stageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<DealResponse>>> GetDeals()
        {
            var deals = await _dealService.GetAllDeals();
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<DealResponse>>> GetActiveDeals()
        {
            var deals = await _dealService.GetActiveDeals();
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<List<DealResponse>>> GetOverdueDeals()
        {
            var deals = await _dealService.GetOverdueDeals();
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("client/{clientId:guid}")]
        public async Task<ActionResult<List<DealResponse>>> GetDealsByClient(Guid clientId)
        {
            var deals = await _dealService.GetDealsByClient(clientId);
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("pipeline/{pipelineId:guid}")]
        public async Task<ActionResult<List<DealResponse>>> GetDealsByPipeline(Guid pipelineId)
        {
            var deals = await _dealService.GetDealsByPipeline(pipelineId);
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("stage/{stageId:guid}")]
        public async Task<ActionResult<List<DealResponse>>> GetDealsByStage(Guid stageId)
        {
            var deals = await _dealService.GetDealsByStage(stageId);
            var response = deals.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DealResponse>> GetDealById(Guid id)
        {
            var deal = await _dealService.GetDealById(id);
            if (deal == null)
                return NotFound();

            return Ok(MapToResponse(deal));
        }

        [HttpGet("{id:guid}/with-details")]
        public async Task<ActionResult<DealResponse>> GetDealWithDetails(Guid id)
        {
            var deal = await _dealService.GetDealWithDetails(id);
            if (deal == null)
                return NotFound();

            return Ok(MapToDetailedResponse(deal));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateDeal([FromBody] DealRequest request)
        {
            var (deal, error) = Deal.Create(
                Guid.NewGuid(),
                request.Title,
                request.ClientId,
                request.PipelineId,
                request.CurrentStageId,
                request.PropertyId,
                request.RequestId,
                request.Notes,
                request.DealAmount,
                request.ExpectedCloseDate
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            try
            {
                var dealId = await _dealService.CreateDeal(deal);
                return Ok(dealId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании сделки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateDeal(Guid id, [FromBody] DealRequest request)
        {
            var existingDeal = await _dealService.GetDealById(id);
            if (existingDeal == null)
                return NotFound();

            var (deal, error) = Deal.Create(
                id,
                request.Title,
                request.ClientId,
                request.PipelineId,
                request.CurrentStageId,
                request.PropertyId,
                request.RequestId,
                request.Notes,
                request.DealAmount,
                request.ExpectedCloseDate
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            deal.StageStartedAt = existingDeal.StageStartedAt;
            deal.StageDeadline = existingDeal.StageDeadline;
            deal.CreatedAt = existingDeal.CreatedAt;
            deal.UpdatedAt = DateTime.UtcNow;
            deal.ClosedAt = existingDeal.ClosedAt;
            deal.IsActive = existingDeal.IsActive;

            try
            {
                var dealId = await _dealService.UpdateDeal(deal);
                return Ok(dealId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении сделки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}/move-stage")]
        public async Task<ActionResult> MoveDealToStage(Guid id, [FromBody] MoveDealStageRequest request)
        {
            try
            {
                var (success, error) = await _dealService.MoveDealToStage(id, request.NewStageId, request.Notes);

                if (success)
                    return Ok("Этап сделки обновлен");
                else
                    return BadRequest(error);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при перемещении сделки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}/close")]
        public async Task<ActionResult> CloseDeal(Guid id)
        {
            try
            {
                var success = await _dealService.CloseDeal(id);
                return success ? Ok("Сделка завершена") : NotFound("Сделка не найдена");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при завершении сделки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}/reopen")]
        public async Task<ActionResult> ReopenDeal(Guid id)
        {
            try
            {
                var success = await _dealService.ReopenDeal(id);
                return success ? Ok("Сделка reopened") : NotFound("Сделка не найдена");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при reopening сделки: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteDeal(Guid id)
        {
            try
            {
                return Ok(await _dealService.DeleteDeal(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении сделки: {ex.Message}");
            }
        }

        [HttpGet("pipeline/{pipelineId:guid}/analytics")]
        public async Task<ActionResult<DealAnalyticsResponse>> GetPipelineAnalytics(Guid pipelineId)
        {
            try
            {
                var analytics = await _dealService.GetDealAnalytics(pipelineId);
                var response = new DealAnalyticsResponse(
                    analytics.TotalDeals,
                    analytics.ActiveDeals,
                    analytics.CompletedDeals,
                    analytics.TotalDealAmount,
                    analytics.AverageDealAmount,
                    analytics.AverageDealDuration
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении аналитики: {ex.Message}");
            }
        }

        [HttpGet("pipeline/{pipelineId:guid}/stages-analytics")]
        public async Task<ActionResult<List<DealStageAnalyticsResponse>>> GetStagesAnalytics(Guid pipelineId)
        {
            try
            {
                var analytics = await _dealService.GetPipelineAnalytics(pipelineId);
                var response = analytics.Select(a => new DealStageAnalyticsResponse(
                    a.StageId,
                    a.StageName,
                    a.DealCount,
                    a.AverageTimeInStage,
                    a.OverdueDeals
                )).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении аналитики этапов: {ex.Message}");
            }
        }

        private DealResponse MapToResponse(Deal deal)
        {
            return new DealResponse(
                deal.Id,
                deal.Title,
                deal.Notes,
                deal.DealAmount,
                deal.ExpectedCloseDate,
                deal.ClientId,
                deal.PipelineId,
                deal.CurrentStageId,
                deal.PropertyId,
                deal.RequestId,
                deal.StageStartedAt,
                deal.StageDeadline,
                deal.CreatedAt,
                deal.UpdatedAt,
                deal.ClosedAt,
                deal.IsActive,
                deal.IsOverdue(),
                null, // Client
                null, // Pipeline  
                null, // CurrentStage
                new List<DealHistoryResponse>() // History
            );
        }

        private DealResponse MapToDetailedResponse(Deal deal)
        {
            ClientResponse? clientResponse = null;
            if (deal.Client != null)
            {
                clientResponse = new ClientResponse(
                    deal.Client.Id,
                    deal.Client.Name,
                    deal.Client.Phone,
                    deal.Client.Email,
                    deal.Client.Source,
                    deal.Client.Notes,
                    deal.Client.CreatedAt
                );
            }

            DealPipelineResponse? pipelineResponse = null;
            if (deal.Pipeline != null)
            {
                pipelineResponse = new DealPipelineResponse(
                    deal.Pipeline.Id,
                    deal.Pipeline.Name,
                    deal.Pipeline.Description,
                    deal.Pipeline.IsActive,
                    deal.Pipeline.CreatedAt,
                    deal.Pipeline.UpdatedAt,
                    deal.Pipeline.Stages.Select(MapToStageResponse).ToList()
                );
            }

            DealStageResponse? stageResponse = null;
            if (deal.CurrentStage != null)
            {
                stageResponse = new DealStageResponse(
                    deal.CurrentStage.Id,
                    deal.CurrentStage.Name,
                    deal.CurrentStage.Description,
                    deal.CurrentStage.Order,
                    deal.CurrentStage.ExpectedDuration,
                    deal.CurrentStage.PipelineId,
                    deal.CurrentStage.CreatedAt
                );
            }

            var historyResponse = deal.History.Select(MapToHistoryResponse).ToList();

            return new DealResponse(
                deal.Id,
                deal.Title,
                deal.Notes,
                deal.DealAmount,
                deal.ExpectedCloseDate,
                deal.ClientId,
                deal.PipelineId,
                deal.CurrentStageId,
                deal.PropertyId,
                deal.RequestId,
                deal.StageStartedAt,
                deal.StageDeadline,
                deal.CreatedAt,
                deal.UpdatedAt,
                deal.ClosedAt,
                deal.IsActive,
                deal.IsOverdue(),
                clientResponse,
                pipelineResponse,
                stageResponse,
                historyResponse
            );
        }

        private DealStageResponse MapToStageResponse(DealStage stage)
        {
            return new DealStageResponse(
                stage.Id,
                stage.Name,
                stage.Description,
                stage.Order,
                stage.ExpectedDuration,
                stage.PipelineId,
                stage.CreatedAt
            );
        }

        private DealHistoryResponse MapToHistoryResponse(DealHistory history)
        {
            DealStageResponse? fromStageResponse = null;
            if (history.FromStage != null)
            {
                fromStageResponse = MapToStageResponse(history.FromStage);
            }

            DealStageResponse? toStageResponse = null;
            if (history.ToStage != null)
            {
                toStageResponse = MapToStageResponse(history.ToStage);
            }

            return new DealHistoryResponse(
                history.Id,
                history.DealId,
                history.FromStageId,
                history.ToStageId,
                history.Notes,
                history.ChangedAt,
                history.TimeInStage,
                fromStageResponse,
                toStageResponse
            );
        }

        [HttpGet("pipeline/{pipelineId:guid}/property-types-analytics")]
        public async Task<ActionResult<List<PropertyTypeAnalyticsResponse>>> GetPropertyTypesAnalytics(Guid pipelineId)
        {
            try
            {
                var analytics = await _dealService.GetPropertyTypeAnalytics(pipelineId);
                var response = analytics.Select(a => new PropertyTypeAnalyticsResponse(
                    a.PropertyType,
                    a.DisplayName,
                    a.DealCount,
                    a.Percentage
                )).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении аналитики по типам недвижимости для воронки {PipelineId}", pipelineId);
                return StatusCode(500, $"Ошибка при получении аналитики по типам недвижимости: {ex.Message}");
            }
        }
    }
}
