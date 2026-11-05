namespace Ejemplo.Banca.Api.Auditoria;

public sealed record EventoAuditoria(string Accion, string Recurso, string UsuarioId, string Resultado, string CorrelationId);

public sealed partial class AuditoriaService
{
    private readonly ILogger<AuditoriaService> _logger;

    public AuditoriaService(ILogger<AuditoriaService> logger) => _logger = logger;

    public void Registrar(EventoAuditoria evento)
    {
        LogAuditoria(_logger, evento.Accion, evento.Recurso, Enmascarar(evento.UsuarioId), evento.Resultado, evento.CorrelationId);
    }

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information,
        Message = "AUDITORIA accion={Accion} recurso={Recurso} usuario={Usuario} resultado={Resultado} correlacion={CorrelationId}")]
    private static partial void LogAuditoria(ILogger logger, string accion, string recurso, string usuario, string resultado, string correlationId);

    private static string Enmascarar(string valor) =>
        valor.Length <= 4 ? "****" : string.Concat(valor.AsSpan(0, 2), "****", valor.AsSpan(valor.Length - 2));
}
