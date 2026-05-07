namespace Biblioteca.Application.Services.Notifications;

public sealed record LoanReceiptData(string UserName, string BookTitle, DateTime DueAt);
public sealed record FineCreatedData(string UserName, string Reason, decimal Amount);
public sealed record ReservationReadyData(string UserName, string BookTitle, DateTime ExpiresAt);

public static class NotificationTemplates
{
    public static (string Subject, string Body) LoanReceipt(LoanReceiptData data) => (
        $"Recibo de préstamo: {data.BookTitle}",
        $"Hola {data.UserName}, registraste el préstamo del libro \"{data.BookTitle}\". Fecha de devolución: {data.DueAt:dd/MM/yyyy}.");

    public static (string Subject, string Body) FineCreated(FineCreatedData data) => (
        $"Multa generada: {data.Reason}",
        $"Hola {data.UserName}, se generó una multa de ${data.Amount:F2} MXN por motivo: {data.Reason}. Por favor regulariza tu situación para poder solicitar nuevos préstamos.");

    public static (string Subject, string Body) ReservationReady(ReservationReadyData data) => (
        $"Reserva lista: {data.BookTitle}",
        $"Hola {data.UserName}, el libro \"{data.BookTitle}\" está disponible para ti. Tienes hasta el {data.ExpiresAt:dd/MM/yyyy HH:mm} para retirar tu ejemplar.");
}
