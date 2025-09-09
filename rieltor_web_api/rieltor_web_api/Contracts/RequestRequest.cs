namespace rieltor_web_api.Contracts
{
    public record RequestRequest(
        Guid? PropertyId,
        string Type,
        string Message,
        string ClientName,
        string ClientPhone,
        string? ClientEmail,
        string Source
        );
}
