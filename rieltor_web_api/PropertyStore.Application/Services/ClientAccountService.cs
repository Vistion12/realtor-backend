using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.Extensions.Logging;

namespace PropertyStore.Application.Services
{
    public class ClientAccountService : IClientAccountService
    {
        private readonly IClientsRepository _clientsRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramService _telegramService; 
        private readonly ILogger<ClientAccountService> _logger; 

        public ClientAccountService(IClientsRepository clientsRepository, IUserRepository userRepository,
            ITelegramService telegramService, 
            ILogger<ClientAccountService> logger) 
        {
            _clientsRepository = clientsRepository;
            _userRepository = userRepository;
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task<(bool success, string error)> ActivateClientAccount(Guid clientId, string temporaryPassword)
        {
            try
            {
                var client = await _clientsRepository.GetById(clientId);
                if (client == null)
                    return (false, "Клиент не найден");

                if (client.HasPersonalAccount)
                    return (false, "Личный кабинет уже активирован");

                // Используем email как логин, если он есть
                var accountLogin = client.Email;
                if (string.IsNullOrEmpty(accountLogin))
                    return (false, "У клиента должен быть email для активации ЛК");

                // НЕ хешируем пароль - храним в открытом виде для Telegram
                client.ActivatePersonalAccount(accountLogin, temporaryPassword);

                // Сохраняем изменения
                var updated = await _clientsRepository.UpdateClientAccountInfo(client);
                if (!updated)
                    return (false, "Ошибка при сохранении данных");

                //  ОТПРАВЛЯЕМ ПАРОЛЬ В TELEGRAM
                await SendPasswordToTelegram(client, temporaryPassword);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка активации: {ex.Message}");
            }
        }

        private async Task SendPasswordToTelegram(Client client, string temporaryPassword)
        {
            try
            {
                var message = $"🔐 <b>АКТИВИРОВАН ЛИЧНЫЙ КАБИНЕТ</b>\n\n" +
                             $"👤 <b>Клиент:</b> {client.Name}\n" +
                             $"📧 <b>Логин:</b> <code>{client.AccountLogin}</code>\n" +
                             $"🔑 <b>Временный пароль:</b> <code>{temporaryPassword}</code>\n\n" +
                             $"⚠️ <i>Передайте пароль клиенту безопасным способом</i>\n" +
                             $"🌐 <i>Ссылка для входа: https://ваш-сайт/client-login</i>";

                var success = await _telegramService.SendMessageAsync(message);

                if (!success)
                {
                    _logger.LogWarning("Не удалось отправить пароль в Telegram для клиента {ClientId}", client.Id);
                }
                else
                {
                    _logger.LogInformation("Пароль отправлен в Telegram для клиента {ClientId}", client.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки пароля в Telegram для клиента {ClientId}", client.Id);
                // НЕ пробрасываем исключение - активация прошла успешно
            }
        }

        public async Task<(bool success, string error)> GiveConsent(Guid clientId, string ipAddress)
        {
            try
            {
                var client = await _clientsRepository.GetById(clientId);
                if (client == null)
                    return (false, "Клиент не найден");

                if (!client.HasPersonalAccount)
                    return (false, "Личный кабинет не активирован");

                client.GiveConsent(ipAddress);

                var updated = await _clientsRepository.UpdateClientAccountInfo(client);
                if (!updated)
                    return (false, "Ошибка при сохранении согласия");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка сохранения согласия: {ex.Message}");
            }
        }

        public async Task<(bool success, string error)> ChangeClientPassword(Guid clientId, string newPassword)
        {
            try
            {
                var client = await _clientsRepository.GetById(clientId);
                if (client == null)
                    return (false, "Клиент не найден");

                if (!client.HasPersonalAccount)
                    return (false, "Личный кабинет не активирован");

                // Хешируем новый пароль
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                client.ChangePassword(newPasswordHash);

                var updated = await _clientsRepository.UpdateClientAccountInfo(client);
                if (!updated)
                    return (false, "Ошибка при смене пароля");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка смены пароля: {ex.Message}");
            }
        }

        public async Task<(bool success, string error)> DeactivateClientAccount(Guid clientId)
        {
            try
            {
                var client = await _clientsRepository.GetById(clientId);
                if (client == null)
                    return (false, "Клиент не найден");

                if (!client.HasPersonalAccount)
                    return (false, "Личный кабинет не активирован");

                client.DeactivateAccount();

                var updated = await _clientsRepository.UpdateClientAccountInfo(client);
                if (!updated)
                    return (false, "Ошибка при деактивации аккаунта");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка деактивации: {ex.Message}");
            }
        }

        public async Task<Client?> GetClientByAccountLogin(string login)
        {
            return await _clientsRepository.GetByAccountLogin(login);
        }
    }
}