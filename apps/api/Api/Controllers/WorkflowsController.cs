using BrowserAgent.Api.Application.DTOs.Workflows;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workflows")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(IWorkflowService workflowService, ILogger<WorkflowsController> logger)
    {
        _workflowService = workflowService;
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
