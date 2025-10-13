namespace rieltor_web_api.Contracts
{
    public record DealHistoryResponse(
        Guid Id,
        Guid DealId,
        Guid FromStageId,
        Guid ToStageId,
        string? Notes,
        DateTime ChangedAt,
        TimeSpan TimeInStage,
        DealStageResponse? FromStage,
        DealStageResponse? ToStage
    );
}
