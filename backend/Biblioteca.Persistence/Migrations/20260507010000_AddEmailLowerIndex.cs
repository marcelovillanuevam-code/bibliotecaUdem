using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Persistence.Migrations;

/// <inheritdoc />
public partial class AddEmailLowerIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "CREATE INDEX IF NOT EXISTS idx_user_contacts_email_lower " +
            "ON user_contacts (LOWER(value)) WHERE type = 'email';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP INDEX IF EXISTS idx_user_contacts_email_lower;");
    }
}
