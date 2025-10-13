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
    public class DealStagesController : ControllerBase
    {
        private readonly IDealStageService _stageService;

        public DealStagesController(IDealStageService stageService)
        {
            _stageService = stageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DealStageResponse>>> GetStages()
        {
            var stages = await _stageService.GetAllStages();
            var response = stages.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("pipeline/{pipelineId:guid}")]
        public async Task<ActionResult<List<DealStageResponse>>> GetStagesByPipeline(Guid pipelineId)
        {
            var stages = await _stageService.GetStagesByPipeline(pipelineId);
            var response = stages.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DealStageResponse>> GetStageById(Guid id)
        {
            var stage = await _stageService.GetStageById(id);
            if (stage == null)
                return NotFound();

            return Ok(MapToResponse(stage));
        }

        [HttpGet("{id:guid}/next")]
        public async Task<ActionResult<DealStageResponse>> GetNextStage(Guid id)
        {
            var nextStage = await _stageService.GetNextStage(id);
            if (nextStage == null)
                return NotFound();

            return Ok(MapToResponse(nextStage));
        }

        [HttpGet("{id:guid}/previous")]
        public async Task<ActionResult<DealStageResponse>> GetPreviousStage(Guid id)
        {
            var previousStage = await _stageService.GetPreviousStage(id);
            if (previousStage == null)
                return NotFound();

            return Ok(MapToResponse(previousStage));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateStage([FromBody] DealStageRequest request)
        {
            var (stage, error) = DealStage.Create(
                Guid.NewGuid(),
                request.Name,
                request.Order,
                request.ExpectedDuration,
                request.PipelineId,
                request.Description
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            try
            {
                var stageId = await _stageService.CreateStage(stage);
                return Ok(stageId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании этапа: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateStage(Guid id, [FromBody] DealStageRequest request)
        {
            var existingStage = await _stageService.GetStageById(id);
            if (existingStage == null)
                return NotFound();

            var (stage, error) = DealStage.Create(
                id,
                request.Name,
                request.Order,
                request.ExpectedDuration,
                request.PipelineId,
                request.Description
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            stage.CreatedAt = existingStage.CreatedAt;

            try
            {
                var stageId = await _stageService.UpdateStage(stage);
                return Ok(stageId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении этапа: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteStage(Guid id)
        {
            try
            {
                return Ok(await _stageService.DeleteStage(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении этапа: {ex.Message}");
            }
        }

        [HttpPut("pipeline/{pipelineId:guid}/reorder")]
        public async Task<ActionResult> ReorderStages(Guid pipelineId, [FromBody] List<Guid> stageIds)
        {
            try
            {
                await _stageService.ReorderStages(pipelineId, stageIds);
                return Ok("Порядок этапов обновлен");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при изменении порядка этапов: {ex.Message}");
            }
        }

        private DealStageResponse MapToResponse(DealStage stage)
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
    }
}
