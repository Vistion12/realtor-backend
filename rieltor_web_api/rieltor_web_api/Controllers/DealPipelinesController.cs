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
    public class DealPipelinesController : ControllerBase
    {
        private readonly IDealPipelineService _pipelineService;

        public DealPipelinesController(IDealPipelineService pipelineService)
        {
            _pipelineService = pipelineService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DealPipelineResponse>>> GetPipelines()
        {
            var pipelines = await _pipelineService.GetAllPipelines();
            var response = pipelines.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<DealPipelineResponse>>> GetActivePipelines()
        {
            var pipelines = await _pipelineService.GetActivePipelines();
            var response = pipelines.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DealPipelineResponse>> GetPipelineById(Guid id)
        {
            var pipeline = await _pipelineService.GetPipelineById(id);
            if (pipeline == null)
                return NotFound();

            return Ok(MapToResponse(pipeline));
        }

        [HttpGet("{id:guid}/with-stages")]
        public async Task<ActionResult<DealPipelineResponse>> GetPipelineWithStages(Guid id)
        {
            var pipeline = await _pipelineService.GetPipelineWithStages(id);
            if (pipeline == null)
                return NotFound();

            return Ok(MapToResponse(pipeline));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreatePipeline([FromBody] DealPipelineRequest request)
        {
            var (pipeline, error) = DealPipeline.Create(
                Guid.NewGuid(),
                request.Name,
                request.Description
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            pipeline.IsActive = request.IsActive;

            try
            {
                var pipelineId = await _pipelineService.CreatePipeline(pipeline);
                return Ok(pipelineId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при создании воронки: {ex.Message}");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdatePipeline(Guid id, [FromBody] DealPipelineRequest request)
        {
            var existingPipeline = await _pipelineService.GetPipelineById(id);
            if (existingPipeline == null)
                return NotFound();

            var (pipeline, error) = DealPipeline.Create(
                id,
                request.Name,
                request.Description
            );

            if (!string.IsNullOrEmpty(error))
                return BadRequest(error);

            pipeline.IsActive = request.IsActive;
            pipeline.CreatedAt = existingPipeline.CreatedAt;

            try
            {
                var pipelineId = await _pipelineService.UpdatePipeline(pipeline);
                return Ok(pipelineId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обновлении воронки: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeletePipeline(Guid id)
        {
            try
            {
                return Ok(await _pipelineService.DeletePipeline(id));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при удалении воронки: {ex.Message}");
            }
        }

        [HttpPost("initialize-defaults")]
        public async Task<ActionResult> InitializeDefaultPipelines()
        {
            try
            {
                var result = await _pipelineService.InitializeDefaultPipelines();
                return result ? Ok("Стандартные воронки созданы") : BadRequest("Ошибка при создании воронок");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при инициализации воронок: {ex.Message}");
            }
        }

        private DealPipelineResponse MapToResponse(DealPipeline pipeline)
        {
            return new DealPipelineResponse(
                pipeline.Id,
                pipeline.Name,
                pipeline.Description,
                pipeline.IsActive,
                pipeline.CreatedAt,
                pipeline.UpdatedAt,
                pipeline.Stages.Select(MapToStageResponse).ToList()
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
    }
}
