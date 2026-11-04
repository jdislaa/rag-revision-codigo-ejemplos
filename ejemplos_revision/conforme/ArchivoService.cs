using System.Security.Cryptography;

namespace Ejemplo.Banca.Api.Archivos;

public sealed class ArchivoService
{
    private static readonly Dictionary<string, byte[]> FirmasPermitidas = new()
    {
        [".pdf"] = "%PDF-"u8.ToArray(),
        [".png"] = [0x89, 0x50, 0x4E, 0x47]
    };

    private const long MaxBytes = 5 * 1024 * 1024;
    private readonly string _directorioAlmacenamiento;

    public ArchivoService(IConfiguration config) =>
        _directorioAlmacenamiento = config["Archivos:Directorio"] ?? throw new InvalidOperationException("Falta Archivos:Directorio.");

    public async Task<string> GuardarAsync(IFormFile archivo, CancellationToken ct)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!FirmasPermitidas.TryGetValue(extension, out var firma)) throw new ArgumentException("Tipo de archivo no permitido.");
        if (archivo.Length is <= 0 or > MaxBytes) throw new ArgumentException("Tamano de archivo invalido.");

        await using var entrada = archivo.OpenReadStream();
        var cabecera = new byte[firma.Length];
        await entrada.ReadExactlyAsync(cabecera, ct);
        if (!cabecera.SequenceEqual(firma)) throw new ArgumentException("El contenido no coincide con el tipo declarado.");
        entrada.Position = 0;

        var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(_directorioAlmacenamiento, nombreSeguro);
        await using var salida = File.Create(ruta);
        await entrada.CopyToAsync(salida, ct);
        return nombreSeguro;
    }
}
