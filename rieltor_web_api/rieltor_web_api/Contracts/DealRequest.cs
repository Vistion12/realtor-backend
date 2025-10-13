namespace rieltor_web_api.Contracts
{
    public record DealRequest(
        string Title,
        string? Notes,
        decimal? DealAmount,
        DateTime? ExpectedCloseDate,
        Guid ClientId,
        Guid PipelineId,
        Guid CurrentStageId,
        Guid? PropertyId,
        Guid? RequestId
    );
}
