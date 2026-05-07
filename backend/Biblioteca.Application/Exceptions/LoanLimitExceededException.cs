namespace Biblioteca.Application.Exceptions;

public sealed class LoanLimitExceededException() : Exception("Límite de préstamos alcanzado para este rol");
