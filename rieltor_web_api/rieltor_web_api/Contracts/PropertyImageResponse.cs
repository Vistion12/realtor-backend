namespace rieltor_web_api.Contracts
{
    public record PropertyImageResponse(
        Guid Id,
        string Url,
        bool IsMain,
        int Order
    );
}
