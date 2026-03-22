using Biblioteca.Application.Interfaces.Common;

namespace Biblioteca.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
