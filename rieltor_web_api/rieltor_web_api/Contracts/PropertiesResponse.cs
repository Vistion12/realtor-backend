namespace rieltor_web_api.Contracts
{
    public record PropertiesResponse(Guid Id, string Title, string Type, decimal Price, string Address, decimal Area,
            int Rooms, string Description, bool IsActive, DateTime CreatedAt);
    
}
