namespace rieltor_web_api.Contracts
{
    public record ClientAuthRequest(
        string Login, // Email как логин
        string Password
    );
}
