using LibraryManagementApi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace LibraryManagementApi.Infrastructure.Configuration;

public class AppUrlProvider(IOptions<ClientAppOptions> options) : IAppUrlProvider
{
    private readonly ClientAppOptions _options = options.Value;

    public string BuildPasswordResetUrl(string email, string token)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var path = _options.PasswordResetPath.TrimStart('/');
        var query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        return $"{baseUrl}/{path}?{query}";
    }
}
