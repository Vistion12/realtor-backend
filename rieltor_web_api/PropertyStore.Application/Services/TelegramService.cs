using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.Application.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramBotSettings _botSettings;

        public TelegramService(HttpClient httpClient, IOptions<TelegramBotSettings> botSettings)
        {
            _httpClient = httpClient;
            _botSettings = botSettings.Value;
        }
        public async Task SendMessageAsync(string message)
        {
            var apiUrl = $"https://api.telegram.org/bot{_botSettings.BotToken}/sendMessage";

            var payload = new
            {
                chat_id = _botSettings.ChatId,
                text = message,
                parse_mode = "HTML"
            };

            var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);

            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Telegram API error: {errorContent}");
            }

            response.EnsureSuccessStatusCode();
        }
    }
}
