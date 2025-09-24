namespace rieltor_web_api.Contracts
{
    public record AuthRequest(
        string Username,
        string Password
    );
}
