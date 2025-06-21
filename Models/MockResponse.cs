namespace HarMockServer.Models;

public record MockResponse
{
    public int StatusCode { get; init; } = 200;
    public Dictionary<string, string> Headers { get; init; } = new();
    public string? Body { get; init; }
    public string ContentType { get; init; } = "application/json";
}