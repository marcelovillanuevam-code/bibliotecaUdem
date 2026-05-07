using Biblioteca.Application.Common.Events;
using Biblioteca.Application.Common.Services;
using Biblioteca.Application.Interfaces.Auth;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Reservations;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Application.Interfaces.Usuarios;
using Biblioteca.Application.Loans.EventHandlers;
using Biblioteca.Application.Notifications.EventHandlers;
using Biblioteca.Application.Options;
using Biblioteca.Application.Reservations.EventHandlers;
using Biblioteca.Application.Services.Auth;
using Biblioteca.Application.Services.Common;
using Biblioteca.Application.Services.Fines;
using Biblioteca.Application.Services.Libros;
using Biblioteca.Application.Services.Loans;
using Biblioteca.Application.Services.Notifications;
using Biblioteca.Application.Services.Reservations;
using Biblioteca.Application.Services.Returns;
using Biblioteca.Application.Services.Usuarios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Biblioteca.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(_ => { }, typeof(DependencyInjection).Assembly);

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<LoansOptions>()
            .Bind(configuration.GetSection(LoansOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Core services
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILibroService, LibroService>();
        services.AddScoped<IBookCopyService, BookCopyService>();

        // Shared cross-module services
        services.AddScoped<IUserEligibilityService, UserEligibilityService>();
        services.AddScoped<IReservationQueryService, ReservationQueryService>();

        // Loans module
        services.AddScoped<ILoanService, LoanService>();

        // Returns/Fines module
        services.AddScoped<FineCalculator>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IFineService, FineService>();
        services.AddScoped<IFineConfigService, FineConfigService>();

        // Reservations/Notifications module
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Domain event handlers
        services.AddScoped<IDomainEventHandler<Common.Events.LoanReturned>, LoanReturnedHandler>();
        services.AddScoped<IDomainEventHandler<Common.Events.LoanReturned>, LoanReturnedReservationHandler>();
        services.AddScoped<IDomainEventHandler<Common.Events.LoanCreated>, LoanCreatedHandler>();
        services.AddScoped<IDomainEventHandler<Common.Events.FineCreated>, FineCreatedHandler>();

        return services;
    }
}
