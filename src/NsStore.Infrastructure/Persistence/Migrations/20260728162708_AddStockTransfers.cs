using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds <c>transfer_in</c> / <c>transfer_out</c> to the native <c>movement_type</c> enum and the
    /// two transfer tables.
    /// </summary>
    /// <remarks>
    /// <para><b>No DML in here may reference the new enum values.</b> On PG 12+ an
    /// <c>ALTER TYPE … ADD VALUE</c> can run inside a transaction, but the new value cannot be
    /// <em>used</em> until that transaction commits — and EF wraps every migration in exactly one.
    /// The transfer tables are safe company: they are created empty and none of their columns is the
    /// enum, so nothing here writes a movement row.</para>
    /// <para><c>Down</c> is not truly reversible: PostgreSQL cannot remove a value from an enum
    /// type. Dropping the tables works; the two enum values stay.</para>
    /// </remarks>
    public partial class AddStockTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_type", "company,individual")
                .Annotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .Annotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .Annotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .Annotation("Npgsql:Enum:payment_status", "credit,paid")
                .Annotation("Npgsql:Enum:user_role", "admin,seller")
                .OldAnnotation("Npgsql:Enum:client_type", "company,individual")
                .OldAnnotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .OldAnnotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .OldAnnotation("Npgsql:Enum:payment_status", "credit,paid")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller");

            migrationBuilder.CreateTable(
                name: "stock_transfers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transfer_date = table.Column<DateOnly>(type: "date", nullable: false),
                    origin_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_branch_id = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    branch_sequence = table.Column<long>(type: "bigint", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_transfers", x => x.id);
                    table.CheckConstraint("ck_stock_transfers_branches_differ", "origin_branch_id <> destination_branch_id");
                    table.CheckConstraint("ck_stock_transfers_total_quantity_positive", "total_quantity > 0");
                    table.ForeignKey(
                        name: "fk_stock_transfers_branches_destination_branch_id",
                        column: x => x.destination_branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfers_branches_origin_branch_id",
                        column: x => x.origin_branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transfer_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_transfer_items", x => x.id);
                    table.CheckConstraint("ck_stock_transfer_items_quantity_positive", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_stock_transfer_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfer_items_stock_transfers_transfer_id",
                        column: x => x.transfer_id,
                        principalTable: "stock_transfers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_items_product_id",
                table: "stock_transfer_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_items_transfer_id",
                table: "stock_transfer_items",
                column: "transfer_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfers_destination_branch_id_transfer_date",
                table: "stock_transfers",
                columns: new[] { "destination_branch_id", "transfer_date" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfers_number",
                table: "stock_transfers",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfers_origin_branch_id_branch_sequence",
                table: "stock_transfers",
                columns: new[] { "origin_branch_id", "branch_sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfers_origin_branch_id_transfer_date",
                table: "stock_transfers",
                columns: new[] { "origin_branch_id", "transfer_date" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfers_transfer_date",
                table: "stock_transfers",
                column: "transfer_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_transfer_items");

            migrationBuilder.DropTable(
                name: "stock_transfers");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_type", "company,individual")
                .Annotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .Annotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale")
                .Annotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .Annotation("Npgsql:Enum:payment_status", "credit,paid")
                .Annotation("Npgsql:Enum:user_role", "admin,seller")
                .OldAnnotation("Npgsql:Enum:client_type", "company,individual")
                .OldAnnotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .OldAnnotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .OldAnnotation("Npgsql:Enum:payment_status", "credit,paid")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller");
        }
    }
}
