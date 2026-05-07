using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_copy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loaned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    renewal_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    issued_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.id);
                    table.ForeignKey(
                        name: "FK_loans_book_copies_book_copy_id",
                        column: x => x.book_copy_id,
                        principalTable: "book_copies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loans_users_issued_by_user_id",
                        column: x => x.issued_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loan_renewals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    renewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    previous_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    new_due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    renewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_renewals", x => x.id);
                    table.ForeignKey(
                        name: "FK_loan_renewals_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_loan_renewals_loan_id",
                table: "loan_renewals",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_issued_by_user_id",
                table: "loans",
                column: "issued_by_user_id");

            migrationBuilder.CreateIndex(
                name: "loans_active_copy_unique",
                table: "loans",
                column: "book_copy_id",
                unique: true,
                filter: "status = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "loans_book_copy_status_idx",
                table: "loans",
                columns: new[] { "book_copy_id", "status" });

            migrationBuilder.CreateIndex(
                name: "loans_user_status_idx",
                table: "loans",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_renewals");

            migrationBuilder.DropTable(
                name: "loans");
        }
    }
}
