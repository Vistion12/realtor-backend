namespace rieltor_web_api.Contracts
{
    public record DealStageRequest(
        string Name,
        string? Description,
        int Order,
        TimeSpan ExpectedDuration,
        Guid PipelineId
    );
}
