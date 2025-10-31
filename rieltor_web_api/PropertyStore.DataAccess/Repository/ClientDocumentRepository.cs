using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.EntityFrameworkCore;
using PropertyStore.DataAccess.Entities;

namespace PropertyStore.DataAccess.Repository
{
    public class ClientDocumentRepository : IClientDocumentRepository
    {
        private readonly PropertyStoreDBContext _context;

        public ClientDocumentRepository(PropertyStoreDBContext context)
        {
            _context = context;
        }

        public async Task<Guid> Create(ClientDocument document)
        {
            var entity = new ClientDocumentEntity
            {
                Id = document.Id,
                ClientId = document.ClientId,
                DealId = document.DealId,
                FileName = document.FileName,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                FileType = document.FileType,
                Category = document.Category,
                UploadedBy = document.UploadedBy,
                UploadedById = document.UploadedById,
                Description = document.Description,
                IsTemplate = document.IsTemplate,
                IsRequired = document.IsRequired,
                RequiredUntil = document.RequiredUntil,
                UploadedAt = document.UploadedAt
            };

            _context.ClientDocuments.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<ClientDocument?> GetById(Guid id)
        {
            var entity = await _context.ClientDocuments
                .Include(d => d.Client)
                .Include(d => d.Deal)
                .FirstOrDefaultAsync(d => d.Id == id);

            return entity == null ? null : MapToDomain(entity);
        }

        public async Task<List<ClientDocument>> GetByClientId(Guid clientId)
        {
            var entities = await _context.ClientDocuments
                .Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<ClientDocument>> GetByClientIdAndCategory(Guid clientId, string category)
        {
            var entities = await _context.ClientDocuments
                .Where(d => d.ClientId == clientId && d.Category == category)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<ClientDocument>> GetRequiredDocuments(Guid clientId)
        {
            var entities = await _context.ClientDocuments
                .Where(d => d.ClientId == clientId && d.IsRequired)
                .OrderBy(d => d.RequiredUntil)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<List<ClientDocument>> GetOverdueDocuments()
        {
            var entities = await _context.ClientDocuments
                .Where(d => d.IsRequired && d.RequiredUntil.HasValue && d.RequiredUntil.Value < DateTime.UtcNow)
                .Include(d => d.Client)
                .OrderBy(d => d.RequiredUntil)
                .ToListAsync();

            return entities.Select(MapToDomain).ToList();
        }

        public async Task<Guid> Delete(Guid id)
        {
            var entity = await _context.ClientDocuments.FindAsync(id);
            if (entity == null)
                return Guid.Empty;

            _context.ClientDocuments.Remove(entity);
            await _context.SaveChangesAsync();
            return id;
        }

        public async Task<bool> DocumentExists(Guid id)
        {
            return await _context.ClientDocuments.AnyAsync(d => d.Id == id);
        }

        private ClientDocument MapToDomain(ClientDocumentEntity entity)
        {
            var (document, error) = ClientDocument.Create(
                entity.Id,
                entity.ClientId,
                entity.FileName,
                entity.FilePath,
                entity.FileUrl,
                entity.FileSize,
                entity.FileType,
                entity.Category,
                entity.DealId,
                entity.UploadedBy,
                entity.UploadedById,
                entity.Description,
                entity.IsTemplate,
                entity.IsRequired,
                entity.RequiredUntil
            );

            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException($"Invalid document data: {error}");

            // UploadedAt уже установлен в конструкторе ClientDocument
            return document;
        }
    }
}