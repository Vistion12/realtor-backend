namespace rieltor_web_api.Contracts
{
    public record AuthResponse(
        string Token,
        string Username,
        string Role,
        DateTime Expires
    );
}
