using Biblioteca.API.Extensions;
using Biblioteca.API.Middlewares;
using Biblioteca.Application;
using Biblioteca.Infrastructure;
using Biblioteca.Persistence;
using Biblioteca.Persistence.Seeding;
using Microsoft.Extensions.Logging.EventLog;

var builder = WebApplication.CreateBuilder(args);

if (OperatingSystem.IsWindows())
{
    builder.Logging.AddFilter<EventLogLoggerProvider>(_ => false);
}

// Falla el arranque si el secret JWT no está configurado o es demasiado corto.
// Configurar via user secrets (dev) o variable de entorno Jwt__SecretKey (prod/docker).
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey no configurada o demasiado corta (minimo 32 caracteres). " +
        "Configurar via user secrets o variable de entorno Jwt__SecretKey.");
}

builder.Services
    .AddApiConfiguration()
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

var app = builder.Build();

if (args.Contains("--init-db", StringComparer.OrdinalIgnoreCase))
{
    await app.Services.InitializeDatabaseAsync();
    return;
}

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
