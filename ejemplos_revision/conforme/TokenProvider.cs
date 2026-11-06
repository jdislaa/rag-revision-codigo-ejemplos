using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Ejemplo.Banca.Integraciones;

public sealed record TokenResponse(string access_token, int expires_in);

public sealed class OAuthTokenProvider
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public OAuthTokenProvider(HttpClient http, IMemoryCache cache, IConfiguration config)
    {
        _http = http;
        _cache = cache;
        _config = config;
    }

    public async Task<AuthenticationHeaderValue> GetBearerAsync(CancellationToken ct)
    {
        var token = await _cache.GetOrCreateAsync("token-cuentas", async entry =>
        {
            var clientId = _config["Integraciones:Cuentas:ClientId"] ?? throw new InvalidOperationException("Falta ClientId.");
            var clientSecret = _config["Integraciones:Cuentas:ClientSecret"] ?? throw new InvalidOperationException("Falta ClientSecret.");

            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = "cuentas.read"
            });

            using var response = await _http.PostAsync("connect/token", form, ct);
            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
                ?? throw new InvalidOperationException("Respuesta de token invalida.");

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Math.Max(30, dto.expires_in - 60));
            return dto.access_token;
        });

        return new AuthenticationHeaderValue("Bearer", token);
    }
}
