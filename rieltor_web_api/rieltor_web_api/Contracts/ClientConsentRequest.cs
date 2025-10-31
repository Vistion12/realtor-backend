namespace rieltor_web_api.Contracts
{
    public record ClientConsentRequest(
        bool AcceptPersonalData,
        bool AcceptDocumentStorage,
        string IpAddress,
        string UserAgent
    );
}
