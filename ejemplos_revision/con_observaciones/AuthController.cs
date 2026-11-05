using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Ejemplo.Banca.Api.Auth;

public interface IUsuarioStore
{
    Task<(string Id, string Clave)?> BuscarPorNombreAsync(string usuario);
}

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string JwtKey = "EJEMPLO-CLAVE-JWT-NO-REAL-0123456789";
    private readonly IUsuarioStore _usuarios;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUsuarioStore usuarios, ILogger<AuthController> logger)
    {
        _usuarios = usuarios;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string usuario, string clave)
    {
        var u = await _usuarios.BuscarPorNombreAsync(usuario);
        if (u is null) return NotFound("El usuario no existe.");
        if (u.Value.Clave != clave) return Unauthorized("Clave incorrecta.");

        _logger.LogInformation("Login exitoso usuario={Usuario} clave={Clave}", usuario, clave);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var token = new JwtSecurityToken(
            claims: new[] { new Claim("sub", u.Value.Id), new Claim("rol", "admin") },
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
