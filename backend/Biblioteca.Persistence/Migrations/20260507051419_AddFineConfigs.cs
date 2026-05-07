using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFineConfigs : Migration
    {
        // GUID fijo para poder hacer rollback reproducible en Down
        private static readonly Guid InitialConfigId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed inicial: tarifas vigentes al arrancar el sistema
            migrationBuilder.InsertData(
                table: "fine_configs",
                columns: new[] { "id", "late_rate_per_day_mxn", "damage_flat_mxn", "loss_flat_mxn", "effective_from" },
                values: new object[] { InitialConfigId, 5.00m, 200.00m, 500.00m, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "fine_configs",
                keyColumn: "id",
                keyValue: InitialConfigId);
        }
    }
}
