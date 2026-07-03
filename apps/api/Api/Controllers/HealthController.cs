using Microsoft.AspNetCore.Mvc;

namespace BrowserAgent.Api.Api.Controllers;

/// <summary>
/// Health check endpoint for monitoring service availability.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the current health status of the API service.
    /// </summary>
    /// <returns>Service health status</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "BrowserAgent.Api",
            version = "1.0.0"
        });
    }
}
