using System.Text.Json;
using Biblioteca.Application.Interfaces.Common;
using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Biblioteca.Persistence.Context;

public sealed class BibliotecaDbContext(
    DbContextOptions<BibliotecaDbContext> options,
    ICurrentUserService currentUserService) : DbContext(options), IUnitOfWork
{
    public DbSet<RegistroAuditoria> AuditLogs => Set<RegistroAuditoria>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanRenewal> LoanRenewals => Set<LoanRenewal>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<Fine> Fines => Set<Fine>();
    public DbSet<FineConfig> FineConfigs => Set<FineConfig>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Autor> Authors => Set<Autor>();
    public DbSet<LibroAutor> BookAuthors => Set<LibroAutor>();
    public DbSet<LibroMateria> BookSubjects => Set<LibroMateria>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Libro> Books => Set<Libro>();
    public DbSet<Ubicacion> Locations => Set<Ubicacion>();
    public DbSet<TokenRestablecimientoContrasena> PasswordResetTokens => Set<TokenRestablecimientoContrasena>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Sesion> Sessions => Set<Sesion>();
    public DbSet<Estado> Statuses => Set<Estado>();
    public DbSet<Materia> Subjects => Set<Materia>();
    public DbSet<AutenticacionUsuario> UserAuth => Set<AutenticacionUsuario>();
    public DbSet<ContactoUsuario> UserContacts => Set<ContactoUsuario>();
    public DbSet<PerfilUsuario> UserProfiles => Set<PerfilUsuario>();
    public DbSet<UsuarioRol> UserRoles => Set<UsuarioRol>();
    public DbSet<Usuario> Users => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BibliotecaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private IDbContextTransaction? _currentTransaction;

    public async Task BeginTransactionAsync(CancellationToken ct)
        => _currentTransaction = await Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.CommitAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_currentTransaction is null) return;
        await _currentTransaction.RollbackAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CreateAuditEntries();
        foreach (var entry in auditEntries)
            AuditLogs.Add(entry);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<RegistroAuditoria> CreateAuditEntries()
    {
        var userId = currentUserService.CurrentUserId;
        var now = DateTime.UtcNow;
        var entries = new List<RegistroAuditoria>();

        foreach (var entityEntry in ChangeTracker.Entries())
        {
            if (entityEntry.Entity is RegistroAuditoria) continue;

            var action = entityEntry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (action is null) continue;

            var (tableName, recordId) = GetAuditInfo(entityEntry.Entity);
            if (tableName is null) continue;

            entries.Add(new RegistroAuditoria
            {
                Id = Guid.NewGuid(),
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                PerformedBy = userId,
                PerformedAt = now,
                ChangedDataJson = BuildChangedDataJson(entityEntry)
            });
        }

        return entries;
    }

    private static string? BuildChangedDataJson(EntityEntry entityEntry)
    {
        var props = entityEntry.Properties.ToList();

        if (entityEntry.State == EntityState.Added)
            return JsonSerializer.Serialize(new
            {
                after = props.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
            });

        if (entityEntry.State == EntityState.Modified)
        {
            var changed = props.Where(p => p.IsModified).ToList();
            if (changed.Count == 0) return null;
            return JsonSerializer.Serialize(new
            {
                before = changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue),
                after = changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
            });
        }

        if (entityEntry.State == EntityState.Deleted)
            return JsonSerializer.Serialize(new
            {
                before = props.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)
            });

        return null;
    }

    private static (string? TableName, Guid? RecordId) GetAuditInfo(object entity) =>
        entity switch
        {
            Usuario u => ("users", u.Id),
            Libro l => ("books", l.Id),
            BookCopy bc => ("book_copies", bc.Id),
            Loan loan => ("loans", loan.Id),
            LoanRenewal lr => ("loan_renewals", lr.Id),
            Return ret => ("returns", ret.Id),
            Fine fine => ("fines", fine.Id),
            Reservation res => ("reservations", res.Id),
            _ => (null, null)
        };
}
