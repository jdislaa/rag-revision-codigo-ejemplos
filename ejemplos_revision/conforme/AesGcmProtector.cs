using System.Security.Cryptography;

namespace Ejemplo.Banca.Api.Seguridad;

public interface IClaveProvider
{
    byte[] ObtenerClave(string keyId);
}

public sealed class AesGcmProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly IClaveProvider _claves;

    public AesGcmProtector(IClaveProvider claves) => _claves = claves;

    public byte[] Proteger(byte[] datos, string keyId, byte[] datosAsociados)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cifrado = new byte[datos.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_claves.ObtenerClave(keyId), TagSize);
        aes.Encrypt(nonce, datos, cifrado, tag, datosAsociados);

        var salida = new byte[NonceSize + TagSize + cifrado.Length];
        nonce.CopyTo(salida, 0);
        tag.CopyTo(salida, NonceSize);
        cifrado.CopyTo(salida, NonceSize + TagSize);
        return salida;
    }

    public byte[] Desproteger(byte[] paquete, string keyId, byte[] datosAsociados)
    {
        if (paquete.Length < NonceSize + TagSize) throw new CryptographicException("Paquete invalido.");

        var nonce = paquete.AsSpan(0, NonceSize);
        var tag = paquete.AsSpan(NonceSize, TagSize);
        var cifrado = paquete.AsSpan(NonceSize + TagSize);
        var claro = new byte[cifrado.Length];

        using var aes = new AesGcm(_claves.ObtenerClave(keyId), TagSize);
        aes.Decrypt(nonce, cifrado, tag, claro, datosAsociados);
        return claro;
    }
}
