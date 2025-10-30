namespace rieltor_web_api.Contracts
{
    public record PropertyTypeAnalyticsResponse(
        string PropertyType,      
        string DisplayName,        
        int DealCount,            
        double Percentage         
    );
}
