using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fine_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    late_rate_per_day_mxn = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    damage_flat_mxn = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    loss_flat_mxn = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fine_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inspection_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    received_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_returns", x => x.id);
                    table.ForeignKey(
                        name: "FK_returns_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_returns_users_received_by_user_id",
                        column: x => x.received_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    return_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    days_late = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fines", x => x.id);
                    table.ForeignKey(
                        name: "FK_fines_returns_return_id",
                        column: x => x.return_id,
                        principalTable: "returns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fines_users_paid_by_user_id",
                        column: x => x.paid_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fines_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_fine_configs_effective_from",
                table: "fine_configs",
                column: "effective_from");

            migrationBuilder.CreateIndex(
                name: "idx_fines_return_id",
                table: "fines",
                column: "return_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_fines_user_status",
                table: "fines",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_fines_paid_by_user_id",
                table: "fines",
                column: "paid_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_returns_received_by_user_id",
                table: "returns",
                column: "received_by_user_id");

            migrationBuilder.CreateIndex(
                name: "uq_returns_loan_id",
                table: "returns",
                column: "loan_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fine_configs");

            migrationBuilder.DropTable(
                name: "fines");

            migrationBuilder.DropTable(
                name: "returns");
        }
    }
}
