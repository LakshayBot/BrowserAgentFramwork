using BrowserAgent.Api.Application.DTOs.Documents;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// List all documents for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _documentService.GetAllAsync(userId, ct);
        return Ok(ApiResponse<List<DocumentDto>>.Ok(result));
    }

    /// <summary>
    /// Get a specific document's metadata.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _documentService.GetByIdAsync(id, userId, ct);
        return Ok(ApiResponse<DocumentDto>.Ok(result));
    }

    /// <summary>
    /// Upload a resume or cover letter document.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<DocumentUploadResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest request)
    {
        var ct = HttpContext.RequestAborted;
        var userId = User.GetUserId();
        var result = await _documentService.UploadAsync(userId, request.File, request.DocumentType, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DocumentUploadResult>.Ok(result));
    }

    /// <summary>
    /// Delete a document.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _documentService.DeleteAsync(id, userId, ct);
        return NoContent();
    }
}
