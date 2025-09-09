namespace rieltor_web_api.Contracts
{
    public record ClientResponse(
        Guid Id,
        string Name,
        string Phone,
        string? Email,
        string Source,
        string? Notes,
        DateTime CreatedAt
    );
}
