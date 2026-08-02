using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "receipt_id",
                table: "payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "receipt_sequence",
                table: "branches",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "payment_receipts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    client_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    branch_sequence = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    receipt_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_receipts", x => x.id);
                    table.CheckConstraint("ck_payment_receipts_total_positive", "total_amount > 0");
                    table.ForeignKey(
                        name: "fk_payment_receipts_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_receipts_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_receipt_id",
                table: "payments",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_receipts_branch_id_branch_sequence",
                table: "payment_receipts",
                columns: new[] { "branch_id", "branch_sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_receipts_client_id_receipt_date",
                table: "payment_receipts",
                columns: new[] { "client_id", "receipt_date" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_receipts_number",
                table: "payment_receipts",
                column: "number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_receipts_receipt_id",
                table: "payments",
                column: "receipt_id",
                principalTable: "payment_receipts",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_receipts_receipt_id",
                table: "payments");

            migrationBuilder.DropTable(
                name: "payment_receipts");

            migrationBuilder.DropIndex(
                name: "ix_payments_receipt_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "receipt_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "receipt_sequence",
                table: "branches");
        }
    }
}
