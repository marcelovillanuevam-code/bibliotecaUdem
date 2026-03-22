namespace Biblioteca.Application.Interfaces.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
