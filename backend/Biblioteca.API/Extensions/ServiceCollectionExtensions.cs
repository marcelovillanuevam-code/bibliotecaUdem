using System.Text.Json.Serialization;
using Biblioteca.API.Services;
using Biblioteca.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace Biblioteca.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddProblemDetails();
        services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Biblioteca UDEM API",
                Version = "v1"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Bearer token. Ejemplo: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    JwtBearerDefaults.AuthenticationScheme,
                    document,
                    null)] = []
            });
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminOnly,
                policy => policy.RequireRole("ADMIN"));

            options.AddPolicy(AuthPolicies.AdminOrLibrarian,
                policy => policy.RequireRole("ADMIN", "LIBRARIAN"));

            options.AddPolicy(AuthPolicies.TreasuryOrAdmin,
                policy => policy.RequireRole("ADMIN", "TREASURY"));

            options.AddPolicy(AuthPolicies.Authenticated,
                policy => policy.RequireAuthenticatedUser());
        });

        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                var origins = corsOrigins.Length > 0
                    ? corsOrigins
                    : ["http://localhost:4200"];

                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }
}
