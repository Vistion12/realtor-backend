namespace rieltor_web_api.Contracts
{
    public record ClientDealResponse(
       Guid Id,
       string Title,
       string? Notes,
       decimal? DealAmount,
       DateTime? ExpectedCloseDate,
       Guid PipelineId,
       string PipelineName,
       Guid CurrentStageId,
       string CurrentStageName,
       DateTime StageStartedAt,
       DateTime? StageDeadline,
       DateTime CreatedAt,
       bool IsActive,
       bool IsOverdue,
       string Status // "active", "completed", "overdue"
   );
}
