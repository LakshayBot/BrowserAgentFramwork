using System.Security.Cryptography;
using BrowserAgent.Api.Application.DTOs.Documents;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Enums;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using BrowserAgent.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        "application/rtf"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".txt", ".rtf"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly AppDbContext _context;
    private readonly IStorageService _storage;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(AppDbContext context, IStorageService storage, ILogger<DocumentService> logger)
    {
        _context = context;
        _storage = storage;
        _logger = logger;
    }

    public async Task<List<DocumentDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentDto
            {
                Id = x.Id,
                DocumentType = x.DocumentType.ToString(),
                DisplayName = x.DisplayName,
                MimeType = x.MimeType,
                FileSize = x.FileSize,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<DocumentDto> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var doc = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (doc is null)
        {
            throw new NotFoundException("Document", id);
        }

        return new DocumentDto
        {
            Id = doc.Id,
            DocumentType = doc.DocumentType.ToString(),
            DisplayName = doc.DisplayName,
            MimeType = doc.MimeType,
            FileSize = doc.FileSize,
            CreatedAt = doc.CreatedAt
        };
    }

    public async Task<DocumentUploadResult> UploadAsync(Guid userId, IFormFile file, string documentType, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new DomainException("FILE_REQUIRED", "A file must be provided");
        }

        if (file.Length > MaxFileSize)
        {
            throw new DomainException("FILE_TOO_LARGE", "File size exceeds the maximum of 10 MB");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
        {
            throw new DomainException("INVALID_FILE_TYPE", $"File type '{ext}' is not supported. Supported: PDF, DOC, DOCX, TXT, RTF");
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            throw new DomainException("INVALID_MIME_TYPE", $"MIME type '{file.ContentType}' is not supported");
        }

        if (!Enum.TryParse<DocumentType>(documentType, true, out var docType))
        {
            throw new DomainException("INVALID_DOCUMENT_TYPE", $"Document type '{documentType}' is not supported. Supported: Resume, CoverLetter");
        }

        string sha256;
        await using (var computeStream = file.OpenReadStream())
        {
            sha256 = Convert.ToHexString(await SHA256.HashDataAsync(computeStream, ct)).ToLowerInvariant();
        }

        var existing = await _context.Documents
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Sha256 == sha256, ct);

        if (existing)
        {
            throw new DomainException("DUPLICATE_DOCUMENT", "A document with the same content already exists");
        }

        var storageDir = $"users/{userId}/documents";
        var storageName = $"{Guid.NewGuid()}{ext}";
        var storagePath = $"{storageDir}/{storageName}";

        await using var uploadStream = file.OpenReadStream();
        await _storage.SaveAsync(storagePath, uploadStream, ct);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentType = docType,
            DisplayName = file.FileName,
            StoragePath = storagePath,
            MimeType = file.ContentType,
            FileSize = file.Length,
            Sha256 = sha256,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Document uploaded: {Id} {Name} ({Size} bytes) for user {UserId}",
            document.Id, document.DisplayName, document.FileSize, userId);

        return new DocumentUploadResult
        {
            Id = document.Id,
            DisplayName = document.DisplayName,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            Sha256 = document.Sha256,
            CreatedAt = document.CreatedAt
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var doc = await _context.Documents
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

        if (doc is null)
        {
            throw new NotFoundException("Document", id);
        }

        await _storage.DeleteAsync(doc.StoragePath, ct);
        _context.Documents.Remove(doc);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Document deleted: {Id} {Name} for user {UserId}",
            id, doc.DisplayName, userId);
    }
}
