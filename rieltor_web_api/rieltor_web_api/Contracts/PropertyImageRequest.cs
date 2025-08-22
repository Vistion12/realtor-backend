namespace rieltor_web_api.Contracts
{
    public record PropertyImageRequest(
        string Url,
        bool IsMain = false,
        int Order = 0
    );
}
