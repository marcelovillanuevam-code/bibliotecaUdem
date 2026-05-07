using System.Text;
using System.Text.RegularExpressions;
using Biblioteca.Application.DTOs.Chatbot;
using Biblioteca.Application.DTOs.Libros;
using Biblioteca.Application.Interfaces.Chatbot;
using Biblioteca.Application.Interfaces.Libros;
using Biblioteca.Application.Interfaces.Loans;
using Biblioteca.Application.Interfaces.Returns;
using Biblioteca.Domain.Entities;

namespace Biblioteca.Application.Services.Chatbot;

// TODO deuda técnica: reemplazar por LLMChatbotProvider (Claude/OpenAI) implementando IChatbotProvider.
public sealed class RuleBasedChatbotProvider(
    ILibroRepository libroRepository,
    IFineRepository fineRepository,
    ILoanRepository loanRepository) : IChatbotProvider
{
    private static readonly Regex SearchBook =
        new(@"\bbuscar?\b\s+(.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MyFines =
        new(@"\b(mis\s+multas?|multas?\s+pendientes?|tengo\s+multas?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex MyLoans =
        new(@"\b(mis\s+pr[eé]stamos?|pr[eé]stamos?\s+activos?|cu[aá]ndo\s+vence|vence\s+mi|pr[eé]stamos?)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Schedule =
        new(@"\b(horarios?|cuando\s+abre|cuando\s+cierra|abr[eo]|cierr[ao])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Location =
        new(@"\b(ubicaci[oó]n|d[oó]nde\s+est[aá]|direcci[oó]n|c[oó]mo\s+llegar)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Contact =
        new(@"\b(contacto|tel[eé]fono|correo|email|comunicar)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private const string ScheduleReply =
        "La biblioteca UDEM abre de lunes a viernes de 7:00 a 22:00, y sábados de 8:00 a 14:00.";

    private const string LocationReply =
        "Estamos en el Edificio de Servicios Académicos, planta baja. Campus UDEM, San Pedro Garza García, N.L.";

    private const string ContactReply =
        "Correo: biblioteca@udem.edu | Teléfono: (81) 8215-1000 ext. 7000";

    private const string DefaultReply =
        "No entendí tu mensaje. Puedes preguntarme:\n• buscar <título>\n• mis multas\n• mis préstamos\n• horario\n• ubicación\n• contacto";

    public async Task<ChatResponse> AskAsync(string message, Guid userId, CancellationToken ct)
    {
        var m = message.Trim();

        var searchMatch = SearchBook.Match(m);
        if (searchMatch.Success)
            return await HandleSearchAsync(searchMatch.Groups[1].Value.Trim(), ct);

        if (MyFines.IsMatch(m))
            return await HandleMyFinesAsync(userId, ct);

        if (MyLoans.IsMatch(m))
            return await HandleMyLoansAsync(userId, ct);

        if (Schedule.IsMatch(m))
            return new ChatResponse(ScheduleReply, null);

        if (Location.IsMatch(m))
            return new ChatResponse(LocationReply, null);

        if (Contact.IsMatch(m))
            return new ChatResponse(ContactReply, null);

        return new ChatResponse(DefaultReply, null);
    }

    private async Task<ChatResponse> HandleSearchAsync(string query, CancellationToken ct)
    {
        var books = await libroRepository.GetAllAsync(new GetLibrosRequest { Title = query }, ct);
        var top3 = books.Take(3).ToList();

        if (top3.Count == 0)
            return new ChatResponse($"No encontré libros para \"{query}\". Intenta con otro título.", null);

        var actions = top3
            // TODO: cambiar a /dashboard/libros/{id} cuando exista la ruta de detalle de libro
            .Select(b => new ChatAction($"Ver: {b.Title}", "/dashboard/catalogo"))
            .ToList();

        var sb = new StringBuilder($"Encontré {top3.Count} libro(s) para \"{query}\":");
        foreach (var book in top3)
        {
            var author = book.Authors.FirstOrDefault()?.FullName ?? "Autor desconocido";
            sb.Append($"\n• {book.Title} — {author}");
        }

        return new ChatResponse(sb.ToString(), actions);
    }

    private async Task<ChatResponse> HandleMyFinesAsync(Guid userId, CancellationToken ct)
    {
        var fines = await fineRepository.GetByUserAsync(userId, FineStatus.PENDING, ct);

        if (fines.Count == 0)
            return new ChatResponse("Sin multas pendientes.", null);

        var total = fines.Sum(f => f.Amount);
        var sb = new StringBuilder($"Tienes {fines.Count} multa(s) pendiente(s) por ${total:0.00}:");
        foreach (var fine in fines)
            sb.Append($"\n• ${fine.Amount:0.00} — {fine.Reason} ({fine.CreatedAt:dd/MM/yyyy})");

        return new ChatResponse(sb.ToString(), null);
    }

    private async Task<ChatResponse> HandleMyLoansAsync(Guid userId, CancellationToken ct)
    {
        var allLoans = await loanRepository.GetByUserAsync(userId, null, ct);
        var active = allLoans
            .Where(l => l.Status == LoanStatus.ACTIVE || l.Status == LoanStatus.OVERDUE)
            .ToList();

        if (active.Count == 0)
            return new ChatResponse("No tienes préstamos activos.", null);

        var sb = new StringBuilder($"Tienes {active.Count} préstamo(s) activo(s):");
        foreach (var loan in active)
        {
            var title = loan.BookCopy?.Book?.Title ?? "Libro desconocido";
            var overdue = loan.Status == LoanStatus.OVERDUE ? " ⚠ VENCIDO" : string.Empty;
            sb.Append($"\n• {title} — vence el {loan.DueAt:dd/MM/yyyy}{overdue}");
        }

        return new ChatResponse(sb.ToString(), null);
    }
}
