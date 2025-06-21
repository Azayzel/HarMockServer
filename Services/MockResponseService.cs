using HarMockServer.Models;
using System.Text;

namespace HarMockServer.Services;

public class MockResponseService : IMockResponseService
{
    private readonly IHarService _harService;
    private readonly ILogger<MockResponseService> _logger;

    public MockResponseService(IHarService harService, ILogger<MockResponseService> logger)
    {
        _harService = harService;
        _logger = logger;
    }

    public async Task<bool> TryServeResponseAsync(HttpContext context, string environmentId)
    {
        var request = context.Request;
        var method = request.Method;
        var path = request.Path + request.QueryString;

        var mockResponse = _harService.GetMockResponse(environmentId, method, path);
        
        if (mockResponse == null)
        {
            _logger.LogWarning("No mock response found for {Method} {Path} in environment {EnvironmentId}",
                method, path, environmentId);
            return false;
        }

        var response = context.Response;
        
        // Set status code
        response.StatusCode = mockResponse.StatusCode;
        
        // Set headers
        foreach (var header in mockResponse.Headers)
        {
            try
            {
                response.Headers[header.Key] = header.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set header {HeaderName}: {HeaderValue}",
                    header.Key, header.Value);
            }
        }
        
        // Set content type
        response.ContentType = mockResponse.ContentType;
        
        // Write response body
        if (!string.IsNullOrEmpty(mockResponse.Body))
        {
            await response.WriteAsync(mockResponse.Body, Encoding.UTF8);
        }
        
        _logger.LogInformation("Served mock response for {Method} {Path} -> {StatusCode}",
            method, path, mockResponse.StatusCode);
        
        return true;
    }
}