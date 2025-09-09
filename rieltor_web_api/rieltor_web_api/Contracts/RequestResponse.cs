namespace rieltor_web_api.Contracts
{
    public record RequestResponse(
        Guid Id,
        Guid ClientId,
        Guid? PropertyId,
        string Type,
        string Status,
        string Message,
        DateTime CreatedAt,
        ClientResponse? Client
        );
}
