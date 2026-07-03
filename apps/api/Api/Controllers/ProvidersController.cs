using BrowserAgent.Api.Application.DTOs.Providers;
using BrowserAgent.Api.Application.Interfaces;
using BrowserAgent.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;
    private readonly ILogger<ProvidersController> _logger;

    public ProvidersController(IProviderService providerService, ILogger<ProvidersController> logger)
    {
        _providerService = providerService;
        _logger = logger;
    }

    /// <summary>
    /// List all AI provider configurations for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProviderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.GetAllAsync(userId, ct);
        return Ok(ApiResponse<List<ProviderDto>>.Ok(result));
    }

    /// <summary>
    /// Get a specific provider configuration.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.GetByIdAsync(id, userId, ct);
        return Ok(ApiResponse<ProviderDto>.Ok(result));
    }

    /// <summary>
    /// Create a new AI provider configuration.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProviderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProviderRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProviderDto>.Ok(result));
    }

    /// <summary>
    /// Update an existing provider configuration.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProviderRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.UpdateAsync(id, userId, request, ct);
        return Ok(ApiResponse<ProviderDto>.Ok(result));
    }

    /// <summary>
    /// Delete a provider configuration.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        await _providerService.DeleteAsync(id, userId, ct);
        return NoContent();
    }

    /// <summary>
    /// Validate connectivity to the AI provider using the stored configuration.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(ApiResponse<ProviderValidationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.ValidateAsync(id, userId, ct);
        return Ok(ApiResponse<ProviderValidationResult>.Ok(result));
    }

    /// <summary>
    /// Set a provider as the default for the current user.
    /// </summary>
    [HttpPost("{id:guid}/default")]
    [ProducesResponseType(typeof(ApiResponse<ProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _providerService.SetDefaultAsync(id, userId, ct);
        return Ok(ApiResponse<ProviderDto>.Ok(result));
    }
}
