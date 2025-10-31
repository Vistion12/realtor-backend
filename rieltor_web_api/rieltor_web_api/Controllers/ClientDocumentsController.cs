using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rieltor_web_api.Controllers
{
    [ApiController]
    [Route("api/client/documents")]
    [Authorize(Roles = "Client")]
    public class ClientDocumentsController : ControllerBase
    {
        private readonly IClientDocumentRepository _documentRepository;
        private readonly IWebHostEnvironment _environment;

        public ClientDocumentsController(IClientDocumentRepository documentRepository, IWebHostEnvironment environment)
        {
            _documentRepository = documentRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult> GetDocuments()
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                var documents = await _documentRepository.GetByClientId(clientId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения документов: {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadDocument(IFormFile file, [FromForm] string? category = "general", [FromForm] string? description = null)
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                if (file == null || file.Length == 0)
                    return BadRequest("Файл не выбран");

                // Проверяем размер файла (макс 10MB)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest("Файл слишком большой (макс. 10MB)");

                // Создаем папку для клиента, если не существует
                var clientFolder = Path.Combine(_environment.ContentRootPath, "uploads", "clients", clientId.ToString());
                if (!Directory.Exists(clientFolder))
                    Directory.CreateDirectory(clientFolder);

                // Генерируем уникальное имя файла
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(clientFolder, fileName);
                var fileUrl = $"/uploads/clients/{clientId}/{fileName}";

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Создаем запись в БД
                var documentId = Guid.NewGuid();
                var (document, error) = ClientDocument.Create(
                    documentId,
                    clientId,
                    file.FileName,
                    filePath,
                    fileUrl,
                    file.Length,
                    file.ContentType,
                    category,
                    uploadedBy: "client",
                    description: description
                );

                if (!string.IsNullOrEmpty(error))
                    return BadRequest(error);

                await _documentRepository.Create(document);

                return Ok(new
                {
                    message = "Файл успешно загружен",
                    documentId = document.Id,
                    fileName = document.FileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка загрузки файла: {ex.Message}");
            }
        }

        [HttpGet("{documentId:guid}/download")]
        public async Task<ActionResult> DownloadDocument(Guid documentId)
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                var document = await _documentRepository.GetById(documentId);
                if (document == null || document.ClientId != clientId)
                    return NotFound("Документ не найден");

                if (!System.IO.File.Exists(document.FilePath))
                    return NotFound("Файл не найден на сервере");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
                return File(fileBytes, document.FileType, document.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка скачивания файла: {ex.Message}");
            }
        }

        [HttpDelete("{documentId:guid}")]
        public async Task<ActionResult> DeleteDocument(Guid documentId)
        {
            try
            {
                var clientId = GetClientIdFromToken();
                if (clientId == Guid.Empty)
                    return Unauthorized();

                var document = await _documentRepository.GetById(documentId);
                if (document == null || document.ClientId != clientId)
                    return NotFound("Документ не найден");

                // Удаляем файл с диска
                if (System.IO.File.Exists(document.FilePath))
                    System.IO.File.Delete(document.FilePath);

                // Удаляем запись из БД
                await _documentRepository.Delete(documentId);

                return Ok(new { message = "Документ удален" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка удаления документа: {ex.Message}");
            }
        }

        private Guid GetClientIdFromToken()
        {
            var clientIdClaim = User.FindFirst("ClientId");
            if (clientIdClaim == null || !Guid.TryParse(clientIdClaim.Value, out var clientId))
                return Guid.Empty;

            return clientId;
        }
    }
}