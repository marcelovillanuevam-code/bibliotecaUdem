using Biblioteca.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Persistence.Migrations;

[DbContext(typeof(BibliotecaDbContext))]
[Migration("20260507010000_AddEmailLowerIndex")]
public class AddEmailLowerIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "CREATE INDEX IF NOT EXISTS idx_user_contacts_email_lower " +
            "ON user_contacts (LOWER(value)) WHERE type = 'email';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP INDEX IF EXISTS idx_user_contacts_email_lower;");
    }
}
