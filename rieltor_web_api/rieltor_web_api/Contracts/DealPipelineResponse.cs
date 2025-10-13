namespace rieltor_web_api.Contracts
{
    public record DealPipelineResponse(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<DealStageResponse> Stages
    );
}
