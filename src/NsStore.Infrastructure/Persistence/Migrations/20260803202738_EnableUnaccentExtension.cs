using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnableUnaccentExtension : Migration
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
                .Annotation("Npgsql:Enum:product_serial_status", "in_stock,removed,sold")
                .Annotation("Npgsql:Enum:serial_event_type", "received,registered,removed,sold,transferred_in,transferred_out")
                .Annotation("Npgsql:Enum:user_role", "admin,seller")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,")
                .OldAnnotation("Npgsql:Enum:client_type", "company,individual")
                .OldAnnotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .OldAnnotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale,transfer_in,transfer_out")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .OldAnnotation("Npgsql:Enum:payment_status", "credit,paid")
                .OldAnnotation("Npgsql:Enum:product_serial_status", "in_stock,removed,sold")
                .OldAnnotation("Npgsql:Enum:serial_event_type", "received,registered,removed,sold,transferred_in,transferred_out")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:product_serial_status", "in_stock,removed,sold")
                .OldAnnotation("Npgsql:Enum:serial_event_type", "received,registered,removed,sold,transferred_in,transferred_out")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,seller")
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
