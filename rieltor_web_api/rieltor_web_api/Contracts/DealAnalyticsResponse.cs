namespace rieltor_web_api.Contracts
{
    public record DealAnalyticsResponse(
        int TotalDeals,
        int ActiveDeals,
        int CompletedDeals,
        decimal TotalDealAmount,
        decimal AverageDealAmount,
        TimeSpan AverageDealDuration
    );
}
