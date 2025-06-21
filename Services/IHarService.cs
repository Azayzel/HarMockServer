using HarMockServer.Models;

namespace HarMockServer.Services;

public interface IHarService
{
    Task<string> CreateEnvironmentFromHarAsync(Stream harStream, string? environmentName = null);
    HarEnvironment? GetEnvironment(string environmentId);
    IEnumerable<HarEnvironment> GetAllEnvironments();
    bool DeleteEnvironment(string environmentId);
    MockResponse? GetMockResponse(string environmentId, string method, string path);
}