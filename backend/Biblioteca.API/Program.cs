using Biblioteca.API.Extensions;
using Biblioteca.API.Middlewares;
using Biblioteca.Application;
using Biblioteca.Infrastructure;
using Biblioteca.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiConfiguration()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();

app.Run();
