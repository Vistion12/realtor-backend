namespace rieltor_web_api.Contracts
{
    public record DealStageAnalyticsResponse(
        Guid StageId,
        string StageName,
        int DealCount,
        TimeSpan AverageTimeInStage,
        int OverdueDeals
    );
}
