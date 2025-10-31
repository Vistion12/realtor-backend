namespace rieltor_web_api.Contracts
{
    public record ClientDealDetailsResponse(
        Guid Id,
        string Title,
        string? Notes,
        decimal? DealAmount,
        DateTime? ExpectedCloseDate,
        string PipelineName,
        CurrentStageInfo CurrentStage,
        DateTime StageStartedAt,
        DateTime? StageDeadline,
        DateTime CreatedAt,
        bool IsActive,
        bool IsOverdue,
        List<DealHistoryEntry> History
    );

    public record CurrentStageInfo(
        Guid Id,
        string Name,
        string? Description,
        int Order
    );

    public record DealHistoryEntry(
        Guid Id,
        string FromStageName,    // ИСПРАВЛЕНО: FromStageName вместо PreviousStageName
        string ToStageName,      // ИСПРАВЛЕНО: ToStageName вместо NewStageName  
        TimeSpan TimeInStage,    // ИСПРАВЛЕНО: TimeInStage вместо TimeInPreviousStage
        DateTime ChangedAt,      // ИСПРАВЛЕНО: ChangedAt вместо CreatedAt
        string? Notes
    );
}
