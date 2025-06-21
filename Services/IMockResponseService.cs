using HarMockServer.Models;

namespace HarMockServer.Services;

public interface IMockResponseService
{
    Task<bool> TryServeResponseAsync(HttpContext context, string environmentId);
}