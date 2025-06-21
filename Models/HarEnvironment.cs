namespace HarMockServer.Models;

public record HarEnvironment
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string OriginalDomain { get; init; } = string.Empty;
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public Dictionary<string, MockResponse> Responses { get; init; } = new();
    public int RequestCount { get; init; }
}