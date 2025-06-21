using HarMockServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace HarMockServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HarController : ControllerBase
{
    private readonly IHarService _harService;
    private readonly ILogger<HarController> _logger;

    public HarController(IHarService harService, ILogger<HarController> logger)
    {
        _harService = harService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadHar(IFormFile harFile, [FromForm] string? environmentName = null)
    {
        if (harFile == null || harFile.Length == 0)
        {
            return BadRequest("Please provide a valid HAR file");
        }

        if (!harFile.FileName.EndsWith(".har", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("File must have .har extension");
        }

        try
        {
            using var stream = harFile.OpenReadStream();
            var environmentId = await _harService.CreateEnvironmentFromHarAsync(stream, environmentName);

            return Ok(new
            {
                EnvironmentId = environmentId,
                Message = "HAR file uploaded successfully",
                MockUrl = $"{Request.Scheme}://{Request.Host}/mock/{environmentId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload HAR file");
            return BadRequest($"Failed to process HAR file: {ex.Message}");
        }
    }

    [HttpGet("environments")]
    public IActionResult GetEnvironments()
    {
        var environments = _harService.GetAllEnvironments()
            .Select(env => new
            {
                env.Id,
                env.Name,
                env.OriginalDomain,
                env.Created,
                ResponseCount = env.Responses.Count,
                MockUrl = $"{Request.Scheme}://{Request.Host}/mock/{env.Id}"
            })
            .ToList();

        return Ok(environments);
    }

    [HttpGet("environments/{environmentId}")]
    public IActionResult GetEnvironment(string environmentId)
    {
        var environment = _harService.GetEnvironment(environmentId);
        
        if (environment == null)
        {
            return NotFound($"Environment {environmentId} not found");
        }

        var result = new
        {
            environment.Id,
            environment.Name,
            environment.OriginalDomain,
            environment.Created,
            ResponseCount = environment.Responses.Count,
            MockUrl = $"{Request.Scheme}://{Request.Host}/mock/{environment.Id}",
            Routes = environment.Responses.Keys.ToList()
        };

        return Ok(result);
    }

    [HttpDelete("environments/{environmentId}")]
    public IActionResult DeleteEnvironment(string environmentId)
    {
        var deleted = _harService.DeleteEnvironment(environmentId);
        
        if (!deleted)
        {
            return NotFound($"Environment {environmentId} not found");
        }

        return Ok(new { Message = $"Environment {environmentId} deleted successfully" });
    }
}