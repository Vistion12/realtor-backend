using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PropertyStore.Application.Services
{
    public class RequestsService : IRequestsService
    {
        private readonly IRequestsRepository _requestsRepository;
        private readonly IClientsService _clientsService;
        private readonly ITelegramService _telegramService;
        private readonly ILogger<RequestsService> _logger;

        public RequestsService(
            IRequestsRepository requestsRepository,
            IClientsService clientsService,
            ITelegramService telegramService,
            ILogger<RequestsService> logger)
        {
            _requestsRepository = requestsRepository;
            _clientsService = clientsService;
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task<Guid> CreateRequest(Request request)
        {
            return await _requestsRepository.Create(request);
        }

        public async Task<Guid> CreateRequestWithClient(
            Guid? propertyId,
            string type,
            string message,
            string clientName,
            string clientPhone,
            string? clientEmail,
            string source)
        {
            // Находим или создаем клиента
            var client = await _clientsService.FindOrCreateClient(
                clientName, clientPhone, clientEmail, source);

            // Создаем заявку
            var requestId = Guid.NewGuid();
            var (request, error) = Request.Create(
                requestId,
                client.Id,
                propertyId,
                type,
                "new", // статус по умолчанию
                message,
                DateTime.UtcNow
            );

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            var createdRequestId = await _requestsRepository.Create(request);

            // ЗАПУСКАЕМ ОТПРАВКУ В TELEGRAM В ФОНЕ - НЕ БЛОКИРУЕМ ОСНОВНОЙ ПОТОК
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendTelegramNotification(createdRequestId, request, client);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Фоновая отправка в Telegram не удалась для заявки {RequestId}", createdRequestId);
                }
            });

            return createdRequestId;
        }

        private async Task SendTelegramNotification(Guid requestId, Request request, Client client)
        {
            try
            {
                var message = $"<b>🏠 НОВАЯ ЗАЯВКА #{requestId.ToString().Substring(0, 8).ToUpper()}</b>\n\n" +
                             $"<b>📋 Тип:</b> {GetRequestTypeLabel(request.Type)}\n" +
                             $"<b>👤 Имя:</b> {client.Name}\n" +
                             $"<b>📞 Телефон:</b> <code>{client.Phone}</code>\n" +
                             $"<b>📧 Email:</b> {client.Email ?? "не указан"}\n" +
                             $"<b>🌐 Источник:</b> {client.Source}\n\n";

                // Парсим дополнительную информацию из JSON
                var additionalInfo = ParseAdditionalInfo(request.Message, request.Type);
                message += additionalInfo;

                message += $"\n<code>📅 {DateTime.Now:dd.MM.yyyy HH:mm}</code>";

                var success = await _telegramService.SendMessageAsync(message);

                if (!success)
                {
                    _logger.LogWarning("Уведомление в Telegram не было отправлено для заявки {RequestId}", requestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при подготовке уведомления для заявки {RequestId}", requestId);
                // НЕ пробрасываем исключение - чтобы не ломать создание заявки
            }
        }

        private string ParseAdditionalInfo(string messageJson, string requestType)
        {
            if (string.IsNullOrEmpty(messageJson))
                return string.Empty;

            try
            {
                var jsonDocument = JsonDocument.Parse(messageJson);
                var root = jsonDocument.RootElement;

                var result = string.Empty;

                if (requestType == "consultation")
                {
                    // Для консультации
                    if (root.TryGetProperty("purpose", out var purpose))
                    {
                        result += $"<b>🎯 Цель:</b> {GetPurposeLabel(purpose.GetString())}\n";
                    }
                    if (root.TryGetProperty("message", out var message) && !string.IsNullOrEmpty(message.GetString()))
                    {
                        result += $"<b>💬 Комментарий:</b>\n{message.GetString()}\n";
                    }
                }
                else if (requestType == "viewing")
                {
                    // Для просмотра
                    if (root.TryGetProperty("preferredDate", out var date) && DateTime.TryParse(date.GetString(), out var preferredDate))
                    {
                        result += $"<b>📅 Желаемая дата:</b> {preferredDate:dd.MM.yyyy HH:mm}\n";
                    }
                    if (root.TryGetProperty("message", out var message) && !string.IsNullOrEmpty(message.GetString()))
                    {
                        result += $"<b>💬 Комментарий:</b>\n{message.GetString()}\n";
                    }
                    // Добавляем информацию об объекте
                    if (root.TryGetProperty("propertyTitle", out var title))
                    {
                        result += $"<b>📍 Объект:</b> {title.GetString()}\n";
                    }
                    if (root.TryGetProperty("propertyAddress", out var address))
                    {
                        result += $"<b>🏢 Адрес объекта:</b> {address.GetString()}\n";
                    }
                }

                return result + "\n";
            }
            catch
            {
                // Если не удалось распарсить JSON, показываем как есть (но обрезаем)
                return $"<b>💬 Сообщение:</b>\n{(messageJson.Length > 200 ? messageJson.Substring(0, 200) + "..." : messageJson)}\n\n";
            }
        }

        private string GetPurposeLabel(string? purpose)
        {
            return purpose?.ToLower() switch
            {
                "buy" => "Купить недвижимость",
                "sell" => "Продать недвижимость",
                "rent" => "Арендовать",
                "other" => "Другое",
                _ => purpose ?? "Не указана"
            };
        }

        private string GetRequestTypeLabel(string type)
        {
            return type.ToLower() switch
            {
                "consultation" => "Консультация",
                "viewing" => "Просмотр объекта",
                _ => type
            };
        }

        public async Task<Guid> DeleteRequest(Guid id)
        {
            return await _requestsRepository.Delete(id);
        }

        public async Task<List<Request>> GetAllRequests()
        {
            return await _requestsRepository.Get();
        }

        public async Task<Request?> GetRequestById(Guid id)
        {
            return await _requestsRepository.GetById(id);
        }

        public async Task<List<Request>> GetRequestsByClientId(Guid clientId)
        {
            return await _requestsRepository.GetByClientId(clientId);
        }

        public async Task<List<Request>> GetRequestsByStatus(string status)
        {
            return await _requestsRepository.GetByStatus(status);
        }

        public async Task<Request?> GetRequestWithDetails(Guid id)
        {
            return await _requestsRepository.GetWithDetails(id);
        }

        public async Task<Guid> UpdateRequestStatus(Guid id, string status)
        {
            return await _requestsRepository.UpdateStatus(id, status);
        }
    }
}