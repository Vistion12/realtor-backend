
namespace AgencyStore.Core.Abstractions
{
    public interface ITelegramService
    {
        Task<bool> SendMessageAsync(string message);
    }
}
