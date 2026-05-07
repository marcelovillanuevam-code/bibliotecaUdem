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

builder.Services
    .AddApiConfiguration()
    .AddApplication()
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
