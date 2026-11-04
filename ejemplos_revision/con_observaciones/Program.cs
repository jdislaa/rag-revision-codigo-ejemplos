using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseCors();
app.MapControllers();
app.MapGet("/debug/config", (IConfiguration config) => config.AsEnumerable());
app.Run();
