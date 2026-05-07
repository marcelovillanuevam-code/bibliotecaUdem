using Biblioteca.Application.Interfaces.Loans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Biblioteca.Infrastructure.Services;

public sealed class OverdueLoansBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueLoansBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var loanService = scope.ServiceProvider.GetRequiredService<ILoanService>();
                await loanService.MarkOverdueAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al marcar préstamos vencidos.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
