using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ejemplo.Banca.Api.Data;

namespace Ejemplo.Banca.Api.Controllers;

public sealed record ConsultaCuentaRequest(string CuentaId);
public sealed record CuentaResponse(string CuentaId, string Estado);

[ApiController]
[Route("api/v1/cuentas")]
[Authorize]
public sealed class CuentasController : ControllerBase
{
    private readonly ICuentaRepository _repositorio;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger<CuentasController> _logger;

    public CuentasController(ICuentaRepository repositorio, IAuthorizationService authorization, ILogger<CuentasController> logger)
    {
        _repositorio = repositorio;
        _authorization = authorization;
        _logger = logger;
    }

    [HttpGet("{cuentaId}")]
    public async Task<IActionResult> Obtener(string cuentaId, CancellationToken ct)
    {
        if (!CuentaIdValidator.EsValido(cuentaId))
            return BadRequest(new ProblemDetails { Title = "Identificador de cuenta invalido." });

        var cuenta = await _repositorio.BuscarAsync(cuentaId, ct);
        if (cuenta is null) return NotFound();

        var decision = await _authorization.AuthorizeAsync(User, cuenta, "ConsultarCuenta");
        if (!decision.Succeeded)
        {
            _logger.LogWarning("Acceso denegado a cuenta {CuentaId} para usuario {UserId}", cuentaId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Forbid();
        }

        return Ok(new CuentaResponse(cuenta.CuentaId, cuenta.Estado));
    }
}

public static class CuentaIdValidator
{
    public static bool EsValido(string? valor) =>
        !string.IsNullOrEmpty(valor) && valor.Length is >= 8 and <= 20 && valor.All(char.IsAsciiDigit);
}
