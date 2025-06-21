namespace HarMockServer.Models;

public record MockRequest
{
    public string Method { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string? QueryString { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();
    public string? Body { get; init; }

    public string GetKey() => $"{Method.ToUpperInvariant()}:{Path}";
}