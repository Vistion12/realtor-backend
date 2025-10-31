namespace rieltor_web_api.Contracts
{
    public record ClientAuthResponse(
        string Token,
        string ClientName,
        string Login,
        DateTime Expires
    );
}
