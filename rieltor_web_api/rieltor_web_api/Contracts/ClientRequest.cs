namespace rieltor_web_api.Contracts
{
    public record ClientRequest(
        string Name,
        string Phone,
        string? Email,
        string Source,
        string? Notes
    );
}
