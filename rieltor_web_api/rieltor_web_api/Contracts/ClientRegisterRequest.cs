namespace rieltor_web_api.Contracts
{
    public record ClientRegisterRequest(
        string TemporaryPassword,
        string NewPassword,
        string ConsentIpAddress
    );
}
