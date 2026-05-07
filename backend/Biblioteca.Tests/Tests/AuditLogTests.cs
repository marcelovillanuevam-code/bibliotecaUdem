using Biblioteca.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Biblioteca.Tests.Tests;

// BUG-07: CreateAuditEntries no debe omitir cambios cuando userId es null
// (la guarda `if (userId is null) return []` fue eliminada en Lote FK-1)
public sealed class AuditLogTests
{
    [Fact]
    public async Task Crear_usuario_genera_fila_insert_en_audit_logs()
    {
        using var ctx = TestDbContextFactory.Create();
        var usuario = TestData.NewUsuario();

        ctx.Users.Add(usuario);
        await ctx.SaveChangesAsync();

        var audits = await ctx.AuditLogs.ToListAsync();
        audits.Should().ContainSingle(a =>
            a.Action == "INSERT" &&
            a.TableName == "users" &&
            a.RecordId == usuario.Id,
            "crear un usuario debe generar un registro INSERT en audit_logs aunque no haya sesión HTTP");
    }

    [Fact]
    public async Task Modificar_usuario_genera_fila_update_en_audit_logs()
    {
        using var ctx = TestDbContextFactory.Create();
        var usuario = TestData.NewUsuario();
        ctx.Users.Add(usuario);
        await ctx.SaveChangesAsync();

        usuario.Username = "usuario_modificado";
        ctx.Users.Update(usuario);
        await ctx.SaveChangesAsync();

        var updateAudits = await ctx.AuditLogs
            .Where(a => a.Action == "UPDATE" && a.TableName == "users")
            .ToListAsync();

        updateAudits.Should().ContainSingle(a => a.RecordId == usuario.Id,
            "modificar un usuario debe generar un registro UPDATE en audit_logs");
    }
}
