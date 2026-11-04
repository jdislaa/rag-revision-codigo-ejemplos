using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Ejemplo.Banca.Api.Controllers;

[ApiController]
[Route("api/v1/cuentas")]
public class CuentasController : ControllerBase
{
    private const string ConnectionString =
        "Server=sql-demo;Database=Banca;User Id=app_banca;Password=EJEMPLO-NO-REAL-123;TrustServerCertificate=True";

    [HttpGet("buscar")]
    public IActionResult Buscar(string numero, string orden)
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            var sql = "SELECT * FROM dbo.Cuenta WHERE Numero = '" + numero + "' ORDER BY " + orden;
            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            var resultado = new List<object>();
            while (reader.Read())
                resultado.Add(new { Numero = reader["Numero"], Saldo = reader["Saldo"], Documento = reader["Documento"], Propietario = reader["PropietarioId"] });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
}
