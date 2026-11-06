using System.Net;
using System.Net.Http.Json;

namespace Ejemplo.Banca.Integraciones;

public sealed record TasaDto(string Moneda, decimal Valor);

public sealed class TasaCambioClient
{
    private readonly HttpClient _http;
    private readonly ILogger<TasaCambioClient> _logger;

    public TasaCambioClient(HttpClient http, ILogger<TasaCambioClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<TasaDto?> ObtenerAsync(string moneda, CancellationToken ct)
    {
        if (moneda.Length != 3 || !moneda.All(char.IsAsciiLetterUpper))
            throw new ArgumentException("Moneda invalida.");

        using var response = await _http.GetAsync($"v1/tasas/{moneda}", ct);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var espera = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
            _logger.LogWarning("Limite de tasa alcanzado. Reintento sugerido en {Segundos}s", espera.TotalSeconds);
            throw new LimiteExcedidoException(espera);
        }

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TasaDto>(ct);
        if (dto is null || dto.Valor <= 0 || dto.Moneda != moneda)
            throw new InvalidOperationException("Respuesta de tasa invalida.");

        return dto;
    }
}

public sealed class LimiteExcedidoException(TimeSpan espera) : Exception("Limite de tasa excedido.")
{
    public TimeSpan Espera { get; } = espera;
}
