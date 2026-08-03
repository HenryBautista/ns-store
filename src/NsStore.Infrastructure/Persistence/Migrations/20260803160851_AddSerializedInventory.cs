using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NsStore.Domain.Enums;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Replaces the single free-text serial on the product row with per-unit tracking.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>products.serial_number</c> described a product <i>type</i>, so it could only ever hold one
    /// number for however many units existed — useless as warranty evidence, and actively confusing
    /// once real per-unit serials print on the same note. Whatever it held is folded into the
    /// description before the column goes, because <c>Down</c> can restore the column but not its
    /// values. No <c>product_serials</c> rows are synthesised from it: nothing records which branch
    /// or which of N units it referred to, and inventing units would break the rule that a branch
    /// never has more identified units than it has stock.
    /// </para>
    /// <para>
    /// EF wraps a migration in one transaction, and PostgreSQL will not let DML use an enum type
    /// created in that same transaction. Nothing here writes to an enum column, which is why it is
    /// safe — do not add a data back-fill touching <c>status</c> or <c>event_type</c> to this file.
    /// </para>
    /// </remarks>
    public partial class AddSerializedInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE products
                SET description = btrim(coalesce(description, '') || ' S/N: ' || serial_number)
                WHERE serial_number IS NOT NULL AND btrim(serial_number) <> '';
                """);

            migrationBuilder.DropColumn(
                name: "serial_number",
                table: "products");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_type", "company,individual")
                .Annotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .Annotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .Annotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .Annotation("Npgsql:Enum:payment_status", "credit,paid")
                .Annotation("Npgsql:Enum:product_serial_status", "in_stock,removed,sold")
                .Annotation("Npgsql:Enum:serial_event_type", "received,registered,removed,sold,transferred_in,transferred_out")
                .Annotation("Npgsql:Enum:user_role", "admin,seller")
                .OldAnnotation("Npgsql:Enum:client_type", "company,individual")
                .OldAnnotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .OldAnnotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .OldAnnotation("Npgsql:Enum:payment_status", "credit,paid")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller");

            migrationBuilder.AddColumn<bool>(
                name: "is_serialized",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "product_serials",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<ProductSerialStatus>(type: "product_serial_status", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    purchase_item_id = table.Column<long>(type: "bigint", nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sale_item_id = table.Column<long>(type: "bigint", nullable: true),
                    sold_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_serials", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_serials_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_serials_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_serials_purchase_items_purchase_item_id",
                        column: x => x.purchase_item_id,
                        principalTable: "purchase_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_serials_sale_items_sale_item_id",
                        column: x => x.sale_item_id,
                        principalTable: "sale_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_serial_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    serial_id = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<SerialEventType>(type: "serial_event_type", nullable: false),
                    branch_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    notes = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_serial_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_serial_events_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_serial_events_product_serials_serial_id",
                        column: x => x.serial_id,
                        principalTable: "product_serials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_serial_events_branch_id",
                table: "product_serial_events",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_serial_events_reference_type_reference_id",
                table: "product_serial_events",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_product_serial_events_serial_id_created_at",
                table: "product_serial_events",
                columns: new[] { "serial_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_product_serials_branch_id",
                table: "product_serials",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_serials_product_id_branch_id_status",
                table: "product_serials",
                columns: new[] { "product_id", "branch_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_product_serials_purchase_item_id",
                table: "product_serials",
                column: "purchase_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_serials_sale_item_id",
                table: "product_serials",
                column: "sale_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_serials_serial_number",
                table: "product_serials",
                column: "serial_number",
                unique: true);

            // A serial is compared the way a human reads it off the sticker, so "ab123" must collide
            // with "AB123". EF cannot express a functional index, hence raw SQL — the same shape the
            // client CI index uses. The exact-case index above stays: EnsureCreated (SQLite, in the
            // tests) never builds this one, so it would otherwise be the only uniqueness the suite sees.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_product_serials_serial_number_ci ON product_serials (lower(serial_number));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_product_serials_serial_number_ci;");

            migrationBuilder.DropTable(
                name: "product_serial_events");

            migrationBuilder.DropTable(
                name: "product_serials");

            migrationBuilder.DropColumn(
                name: "is_serialized",
                table: "products");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_type", "company,individual")
                .Annotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .Annotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .Annotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .Annotation("Npgsql:Enum:payment_status", "credit,paid")
                .Annotation("Npgsql:Enum:user_role", "admin,seller")
                .OldAnnotation("Npgsql:Enum:client_type", "company,individual")
                .OldAnnotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .OldAnnotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .OldAnnotation("Npgsql:Enum:payment_status", "credit,paid")
                .OldAnnotation("Npgsql:Enum:product_serial_status", "in_stock,removed,sold")
                .OldAnnotation("Npgsql:Enum:serial_event_type", "received,registered,removed,sold,transferred_in,transferred_out")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller");

            migrationBuilder.AddColumn<string>(
                name: "serial_number",
                table: "products",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }
    }
}
