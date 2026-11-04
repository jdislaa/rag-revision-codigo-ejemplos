using System.Security.Cryptography;
using System.Text;

namespace Ejemplo.Banca.Api.Servicios;

public class UtilidadesService
{
    private readonly HttpClient _http = new();

    public string HashClave(string password)
    {
        using var md5 = MD5.Create();
        return Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    public async Task<string> DescargarAsync(string url) =>
        await _http.GetStringAsync(url);

    public async Task GuardarArchivoAsync(IFormFile archivo)
    {
        var ruta = Path.Combine("C:\\wwwroot\\uploads", archivo.FileName);
        await using var stream = File.Create(ruta);
        await archivo.CopyToAsync(stream);
    }

    public string GenerarCodigoOtp()
    {
        var random = new Random();
        return random.Next(0, 999999).ToString("D6");
    }
}
