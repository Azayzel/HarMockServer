using HarMockServer.Services;

namespace HarMockServer.Middleware;

public class HarMockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMockResponseService _mockResponseService;
    private readonly ILogger<HarMockMiddleware> _logger;

    public HarMockMiddleware(RequestDelegate next, IMockResponseService mockResponseService, ILogger<HarMockMiddleware> logger)
    {
        _next = next;
        _mockResponseService = mockResponseService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        // Check if this is a mock request
        if (path?.StartsWith("/mock/", StringComparison.OrdinalIgnoreCase) == true)
        {
            var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (pathSegments.Length >= 2)
            {
                var environmentId = pathSegments[1]; // /mock/{environmentId}/...
                var mockPath = "/" + string.Join("/", pathSegments.Skip(2)); // Remaining path
                
                // Update the request path to remove the /mock/{environmentId} prefix
                context.Request.Path = mockPath;
                context.Request.PathBase = $"/mock/{environmentId}";
                
                // Try to serve the mock response
                var served = await _mockResponseService.TryServeResponseAsync(context, environmentId);
                
                if (served)
                {
                    return; // Response was served, don't continue pipeline
                }
                
                // If no mock found, return 404
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"No mock response found for {context.Request.Method} {mockPath} in environment {environmentId}");
                return;
            }
        }

        // Continue to next middleware
        await _next(context);
    }
}