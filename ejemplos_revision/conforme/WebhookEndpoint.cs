using System.Security.Cryptography;
using System.Text;

namespace Ejemplo.Banca.Api.Webhooks;

public interface IEventoDeduplicador
{
    Task<bool> RegistrarSiEsNuevoAsync(string eventId, CancellationToken ct);
}

public static class WebhookEndpoint
{
    private const int MaxBodyBytes = 64 * 1024;

    public static async Task<IResult> Procesar(HttpRequest request, IConfiguration config, IEventoDeduplicador dedup, CancellationToken ct)
    {
        if (request.ContentLength is null or > MaxBodyBytes) return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

        var timestamp = request.Headers["X-Timestamp"].ToString();
        var signature = request.Headers["X-Signature"].ToString();
        var eventId = request.Headers["X-Event-Id"].ToString();
        if (!long.TryParse(timestamp, out var unixSeconds) || string.IsNullOrEmpty(eventId)) return Results.BadRequest();

        var edad = Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - unixSeconds);
        if (edad > 300) return Results.Unauthorized();

        using var reader = new StreamReader(request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync(ct);

        var secret = config["Webhooks:Cuentas:Secret"] ?? throw new InvalidOperationException("Secreto de webhook no configurado.");
        var esperado = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{body}"));

        byte[] recibido;
        try { recibido = Convert.FromHexString(signature); } catch (FormatException) { return Results.Unauthorized(); }
        if (!CryptographicOperations.FixedTimeEquals(esperado, recibido)) return Results.Unauthorized();

        if (!await dedup.RegistrarSiEsNuevoAsync(eventId, ct)) return Results.Ok();

        return Results.Accepted();
    }
}
