using System.Security.Cryptography;

namespace Ejemplo.Banca.Api.Seguridad;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iteraciones = 600_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, HashSize);
        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string almacenado)
    {
        var partes = almacenado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones)) return false;
        var salt = Convert.FromBase64String(partes[1]);
        var esperado = Convert.FromBase64String(partes[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
        return CryptographicOperations.FixedTimeEquals(actual, esperado);
    }
}
