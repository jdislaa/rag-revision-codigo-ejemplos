using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Ejemplo.Banca.Integraciones;

public sealed record EstadoCuentaDto(string CuentaId, string Estado);

public interface ITokenProvider
{
    Task<AuthenticationHeaderValue> GetBearerAsync(CancellationToken ct);
}

public sealed class EstadoCuentaClient
{
    private readonly HttpClient _http;
    private readonly ITokenProvider _tokens;
    private readonly ILogger<EstadoCuentaClient> _logger;

    public EstadoCuentaClient(HttpClient http, ITokenProvider tokens, ILogger<EstadoCuentaClient> logger)
    {
        _http = http;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<EstadoCuentaDto?> ObtenerAsync(string cuentaId, string correlationId, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/cuentas/{Uri.EscapeDataString(cuentaId)}");
        request.Headers.Authorization = await _tokens.GetBearerAsync(ct);
        request.Headers.Add("X-Correlation-Id", correlationId);

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Servicio de cuentas respondio {Status}. Correlacion {CorrelationId}", (int)response.StatusCode, correlationId);
            throw new IntegracionNoDisponibleException("Servicio de cuentas no disponible.");
        }

        if (response.Content.Headers.ContentType?.MediaType != "application/json" ||
            response.Content.Headers.ContentLength is > 1_048_576)
            throw new IntegracionNoDisponibleException("Respuesta invalida del servicio de cuentas.");

        var dto = await response.Content.ReadFromJsonAsync<EstadoCuentaDto>(ct);
        if (dto is null || string.IsNullOrWhiteSpace(dto.CuentaId) || string.IsNullOrWhiteSpace(dto.Estado))
            throw new IntegracionNoDisponibleException("Contrato invalido del servicio de cuentas.");

        return dto;
    }
}

public sealed class IntegracionNoDisponibleException(string message) : Exception(message);
