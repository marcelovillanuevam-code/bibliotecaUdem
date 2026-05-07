namespace Biblioteca.Application.Interfaces.Common;

public interface ICurrentUserService
{
    Guid? CurrentUserId { get; }
}
