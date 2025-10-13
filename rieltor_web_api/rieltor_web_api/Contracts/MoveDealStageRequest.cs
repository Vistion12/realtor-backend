namespace rieltor_web_api.Contracts
{
    public record MoveDealStageRequest(
        Guid NewStageId,
        string? Notes
    );
}
