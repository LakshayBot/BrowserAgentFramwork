using BrowserAgent.Api.Application.DTOs.Workflows;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Entities;
using BrowserAgent.Api.Domain.Exceptions;
using BrowserAgent.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workflows")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly AppDbContext _db;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowService workflowService, AppDbContext db, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new workflow from a job URL.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// Start executing a workflow using its plugin.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.StartAsync(id, userId, ct);
        return Ok(ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// Get workflow details by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.GetByIdAsync(id, userId, ct);
        return Ok(ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// List workflows for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<WorkflowSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.GetAllAsync(userId, page, pageSize, ct);
        return Ok(ApiResponse<List<WorkflowSummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Pause a running workflow.
    /// </summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.PauseAsync(id, userId, ct);
        return Ok(ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// Resume a paused workflow.
    /// </summary>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.ResumeAsync(id, userId, ct);
        return Ok(ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// Check if a workflow is currently paused awaiting human verification.
    /// </summary>
    [HttpGet("{id:guid}/verification-status")]
    [ProducesResponseType(typeof(ApiResponse<VerificationStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVerificationStatus(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var workflow = await _workflowService.GetByIdAsync(id, userId, ct);

        var isAwaitingVerification = workflow.Status == "Paused"
            && (workflow.CurrentStep?.StartsWith("AwaitingHumanVerification") == true);

        var verificationType = isAwaitingVerification
            ? workflow.CurrentStep?.Split(':').LastOrDefault()
            : null;

        var response = new VerificationStatusResponse
        {
            IsAwaitingVerification = isAwaitingVerification,
            VerificationType = verificationType,
            WorkflowStatus = workflow.Status,
            CurrentStep = workflow.CurrentStep
        };

        return Ok(ApiResponse<VerificationStatusResponse>.Ok(response));
    }

    /// <summary>
    /// Cancel a workflow.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<WorkflowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _workflowService.CancelAsync(id, userId, ct);
        return Ok(ApiResponse<WorkflowDto>.Ok(result));
    }

    /// <summary>
    /// Get debug logs for a workflow.
    /// </summary>
    [HttpGet("{id:guid}/logs")]
    [ProducesResponseType(typeof(ApiResponse<List<WorkflowLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs(Guid id, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var exists = await _db.Workflows.AnyAsync(x => x.Id == id && x.UserId == userId, ct);
        if (!exists) return NotFound();

        var logs = await _db.WorkflowLogs
            .AsNoTracking()
            .Where(x => x.WorkflowId == id)
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .OrderBy(x => x.Timestamp)
            .Select(x => new WorkflowLogDto
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                Level = x.Level,
                StepName = x.StepName,
                Message = x.Message,
                Data = x.Data,
                ScreenshotUrl = x.ScreenshotPath != null
                    ? $"/api/v1/workflows/{id}/screenshots/{x.Id}"
                    : null
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<WorkflowLogDto>>.Ok(logs));
    }

    /// <summary>
    /// Get a screenshot captured during workflow execution.
    /// </summary>
    [HttpGet("{id:guid}/screenshots/{logId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreenshot(Guid id, Guid logId, CancellationToken ct)
    {
        var log = await _db.WorkflowLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == logId && x.WorkflowId == id && x.ScreenshotPath != null, ct);

        if (log?.ScreenshotPath is null) return NotFound();

        if (!System.IO.File.Exists(log.ScreenshotPath))
            return NotFound("Screenshot file not found on disk.");

        var contentType = "image/png";
        return PhysicalFile(log.ScreenshotPath, contentType);
    }

    /// <summary>
    /// Delete a workflow.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _workflowService.DeleteAsync(id, userId, ct);
        return NoContent();
    }
}
