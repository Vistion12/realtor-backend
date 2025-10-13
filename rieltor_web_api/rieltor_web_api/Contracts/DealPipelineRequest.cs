namespace rieltor_web_api.Contracts
{
    public record DealPipelineRequest(
        string Name,
        string? Description,
        bool IsActive
    );
}
