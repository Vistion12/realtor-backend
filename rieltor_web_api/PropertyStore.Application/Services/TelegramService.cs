using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(
            HttpClient httpClient,
            IOptions<TelegramBotSettings> botSettings,
            ILogger<TelegramService> logger)
        {
            _httpClient = httpClient;
            _botSettings = botSettings.Value;
            _logger = logger;

            // Настраиваем timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            const int maxRetries = 3;
            const int baseDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("Попытка {Attempt} отправки в Telegram", attempt);

                    var apiUrl = $"https://api.telegram.org/bot{_botSettings.BotToken}/sendMessage";

                    var payload = new
                    {
                        chat_id = _botSettings.ChatId,
                        text = message,
                        parse_mode = "HTML"
                    };

                    using var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Уведомление отправлено в Telegram");
                        return true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Ошибка Telegram API (попытка {Attempt}): {Error}", attempt, errorContent);

                        // Если ошибка клиента (4xx) - не повторяем
                        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            _logger.LogError("Критическая ошибка Telegram API, прекращаем попытки");
                            return false;
                        }
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning("Таймаут подключения к Telegram (попытка {Attempt})", attempt);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning("Ошибка сети Telegram (попытка {Attempt}): {Message}", attempt, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Неожиданная ошибка Telegram (попытка {Attempt}): {Message}", attempt, ex.Message);
                }

                // Если это не последняя попытка - ждем перед повторением
                if (attempt < maxRetries)
                {
                    var delay = baseDelayMs * attempt; // Exponential backoff
                    _logger.LogInformation("Ждем {Delay}ms перед повторной попыткой", delay);
                    await Task.Delay(delay);
                }
            }

            _logger.LogError("❌ Все попытки отправки в Telegram исчерпаны");
            return false;
        }
    }
}