using Ejemplo.Banca.Integraciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(o => o.AddPolicy("ConsultarCuenta", p => p.RequireAuthenticatedUser()));

builder.Services.AddHttpClient<EstadoCuentaClient>(c =>
{
    var baseUrl = builder.Configuration["Integraciones:Cuentas:BaseUrl"]
        ?? throw new InvalidOperationException("Falta Integraciones:Cuentas:BaseUrl.");
    if (!baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("La integracion debe usar HTTPS.");
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = TimeSpan.FromSeconds(10);
}).AddStandardResilienceHandler();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogError(exception, "Error no controlado. Traza {TraceId}", context.TraceIdentifier);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ProblemDetails { Title = "Error interno.", Extensions = { ["traceId"] = context.TraceIdentifier } });
}));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
