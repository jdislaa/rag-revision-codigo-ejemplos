using System.Data;
using Microsoft.Data.SqlClient;

namespace Ejemplo.Banca.Api.Data;

public sealed record Cuenta(string CuentaId, string Estado, string PropietarioId);

public interface ICuentaRepository
{
    Task<Cuenta?> BuscarAsync(string cuentaId, CancellationToken ct);
}

public sealed class CuentaRepository : ICuentaRepository
{
    private readonly string _connectionString;

    public CuentaRepository(IConfiguration configuration) =>
        _connectionString = configuration.GetConnectionString("Banca")
            ?? throw new InvalidOperationException("Falta la configuracion de conexion 'Banca'.");

    public async Task<Cuenta?> BuscarAsync(string cuentaId, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand("dbo.usp_ConsultarCuenta", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 15
        };
        command.Parameters.Add("@CuentaId", SqlDbType.VarChar, 20).Value = cuentaId;

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new Cuenta(reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }
}
