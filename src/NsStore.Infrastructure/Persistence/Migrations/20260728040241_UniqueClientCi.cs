using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UniqueClientCi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_clients_ci",
                table: "clients",
                column: "ci",
                unique: true,
                filter: "ci IS NOT NULL AND deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clients_ci",
                table: "clients");
        }
    }
}
