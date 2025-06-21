using HarSharp;
using HarMockServer.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace HarMockServer.Services;

public class HarService : IHarService
{
    private readonly ConcurrentDictionary<string, HarEnvironment> _environments = new();
    private readonly ILogger<HarService> _logger;

    public HarService(ILogger<HarService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateEnvironmentFromHarAsync(Stream harStream, string? environmentName = null)
    {
        try
        {
            // Read HAR content
            using var reader = new StreamReader(harStream);
            var harContent = await reader.ReadToEndAsync();
            
            // Parse HAR file
            var har = HarConvert.Deserialize(harContent);
            
            // Generate environment ID
            var environmentId = Guid.NewGuid().ToString("N")[..8];
            
            // Extract domain from first entry
            var firstEntry = har.Log.Entries.FirstOrDefault();
            var originalDomain = firstEntry?.Request.Url.Host ?? "unknown";
            
            // Create response dictionary
            var responses = new Dictionary<string, MockResponse>();
            
            foreach (var entry in har.Log.Entries)
            {
                var request = entry.Request;
                var response = entry.Response;
                
                // Create mock request key
                var path = request.Url.PathAndQuery;
                var method = request.Method.ToUpperInvariant();
                var key = $"{method}:{path}";
                
                // Extract response headers
                var headers = response.Headers
                    .Where(h => !IsHopByHopHeader(h.Name))
                    .ToDictionary(h => h.Name, h => h.Value, StringComparer.OrdinalIgnoreCase);
                
                // Get content type
                var contentType = headers.GetValueOrDefault("Content-Type", "application/json");
                
                // Create mock response
                var mockResponse = new MockResponse
                {
                    StatusCode = response.Status,
                    Headers = headers,
                    Body = response.Content?.Text,
                    ContentType = contentType
                };
                
                // Use the first occurrence if duplicates exist
                responses.TryAdd(key, mockResponse);
            }
            
            // Create environment
            var environment = new HarEnvironment
            {
                Id = environmentId,
                Name = environmentName ?? $"Environment-{environmentId}",
                OriginalDomain = originalDomain,
                Responses = responses
            };
            
            _environments[environmentId] = environment;
            
            _logger.LogInformation("Created HAR environment {EnvironmentId} with {ResponseCount} responses from domain {Domain}",
                environmentId, responses.Count, originalDomain);
            
            return environmentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create environment from HAR file");
            throw new InvalidOperationException("Failed to parse HAR file", ex);
        }
    }

    public HarEnvironment? GetEnvironment(string environmentId)
    {
        _environments.TryGetValue(environmentId, out var environment);
        return environment;
    }

    public IEnumerable<HarEnvironment> GetAllEnvironments()
    {
        return _environments.Values.ToList();
    }

    public bool DeleteEnvironment(string environmentId)
    {
        return _environments.TryRemove(environmentId, out _);
    }

    public MockResponse? GetMockResponse(string environmentId, string method, string path)
    {
        if (!_environments.TryGetValue(environmentId, out var environment))
            return null;

        var key = $"{method.ToUpperInvariant()}:{path}";
        
        // Try exact match first
        if (environment.Responses.TryGetValue(key, out var response))
        {
            return response;
        }
        
        // Try path-only match (ignoring query parameters)
        var pathOnly = path.Split('?')[0];
        var pathOnlyKey = $"{method.ToUpperInvariant()}:{pathOnly}";
        
        return environment.Responses.TryGetValue(pathOnlyKey, out response) ? response : null;
    }

    private static bool IsHopByHopHeader(string headerName)
    {
        // Headers that shouldn't be forwarded
        var hopByHopHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade", "Content-Length"
        };
        
        return hopByHopHeaders.Contains(headerName);
    }
}