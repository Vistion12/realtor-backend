namespace rieltor_web_api.Contracts
{
    public record DealStageResponse(
        Guid Id,
        string Name,
        string? Description,
        int Order,
        TimeSpan ExpectedDuration,
        Guid PipelineId,
        DateTime CreatedAt
    );
}
