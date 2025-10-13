namespace rieltor_web_api.Contracts
{
    public record DealResponse(
        Guid Id,
        string Title,
        string? Notes,
        decimal? DealAmount,
        DateTime? ExpectedCloseDate,
        Guid ClientId,
        Guid PipelineId,
        Guid CurrentStageId,
        Guid? PropertyId,
        Guid? RequestId,
        DateTime StageStartedAt,
        DateTime? StageDeadline,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime? ClosedAt,
        bool IsActive,
        bool IsOverdue,
        ClientResponse? Client,
        DealPipelineResponse? Pipeline,
        DealStageResponse? CurrentStage,
        List<DealHistoryResponse> History
    );
}
