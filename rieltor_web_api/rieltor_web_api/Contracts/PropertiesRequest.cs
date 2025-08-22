namespace rieltor_web_api.Contracts
{
    public record PropertiesRequest(
        string Title,
        string Type,
        decimal Price,
        string Address,
        decimal Area,
            int Rooms,
            string Description,
            bool IsActive
        );

}
