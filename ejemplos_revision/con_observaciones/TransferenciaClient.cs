using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ejemplo.Banca.Integraciones;

public class TransferenciaClient
{
    private const string ApiKey = "EJEMPLO-API-KEY-NO-REAL";
    private readonly ILogger<TransferenciaClient> _logger;

    public TransferenciaClient(ILogger<TransferenciaClient> logger) => _logger = logger;

    public async Task<dynamic?> EnviarAsync(object transferencia)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        for (var intento = 0; intento < 10; intento++)
        {
            try
            {
                using var client = new HttpClient(handler);
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                var json = JsonSerializer.Serialize(transferencia);
                _logger.LogInformation("Enviando transferencia {Body} con token {Token}", json, ApiKey);

                var url = "http://pagos.ejemplo.local/api/transferencias?apiKey=" + ApiKey;
                var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                var texto = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<dynamic>(texto);
            }
            catch
            {
            }
        }

        return null;
    }
}
