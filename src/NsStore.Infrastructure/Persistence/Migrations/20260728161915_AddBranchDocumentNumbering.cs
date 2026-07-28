using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Gives sales and purchases a per-branch correlative and a rendered folio.
    /// </summary>
    /// <remarks>
    /// Hand-ordered like <c>AddBranches</c>. The scaffolded version added the columns NOT NULL with
    /// an empty-string default and then built a unique index over them, which collides on the second
    /// pre-existing row. Columns go in nullable, get backfilled by <c>row_number()</c>, and only then
    /// become NOT NULL and unique.
    /// </remarks>
    public partial class AddBranchDocumentNumbering : Migration
    {
        private static readonly string[] NumberedTables = ["sales", "purchases"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in NumberedTables)
            {
                migrationBuilder.AddColumn<long>(name: "branch_sequence", table: table, type: "bigint", nullable: true);
                migrationBuilder.AddColumn<string>(
                    name: "number", table: table, type: "character varying(24)", maxLength: 24, nullable: true);
            }

            // Number the existing documents per branch in business order, then move each branch's
            // counter to its highest issued value so the next insert continues the series.
            migrationBuilder.Sql(
                """
                WITH numbered AS (
                  SELECT id, branch_id,
                         row_number() OVER (PARTITION BY branch_id ORDER BY sale_date, id) AS seq
                  FROM sales
                )
                UPDATE sales s
                SET branch_sequence = n.seq,
                    number = b.code || '-' || lpad(n.seq::text, 6, '0')
                FROM numbered n
                JOIN branches b ON b.id = n.branch_id
                WHERE s.id = n.id;

                UPDATE branches b
                SET sale_sequence = COALESCE((SELECT max(branch_sequence) FROM sales WHERE branch_id = b.id), 0);
                """);

            migrationBuilder.Sql(
                """
                WITH numbered AS (
                  SELECT id, branch_id,
                         row_number() OVER (PARTITION BY branch_id ORDER BY purchase_date, id) AS seq
                  FROM purchases
                )
                UPDATE purchases p
                SET branch_sequence = n.seq,
                    number = b.code || '-' || lpad(n.seq::text, 6, '0')
                FROM numbered n
                JOIN branches b ON b.id = n.branch_id
                WHERE p.id = n.id;

                UPDATE branches b
                SET purchase_sequence = COALESCE((SELECT max(branch_sequence) FROM purchases WHERE branch_id = b.id), 0);
                """);

            foreach (var table in NumberedTables)
            {
                migrationBuilder.AlterColumn<long>(
                    name: "branch_sequence",
                    table: table,
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint",
                    oldNullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "number",
                    table: table,
                    type: "character varying(24)",
                    maxLength: 24,
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "character varying(24)",
                    oldMaxLength: 24,
                    oldNullable: true);
            }

            migrationBuilder.CreateIndex(
                name: "ix_sales_branch_id_branch_sequence",
                table: "sales",
                columns: ["branch_id", "branch_sequence"],
                unique: true);

            migrationBuilder.CreateIndex(name: "ix_sales_number", table: "sales", column: "number", unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_branch_id_branch_sequence",
                table: "purchases",
                columns: ["branch_id", "branch_sequence"],
                unique: true);

            migrationBuilder.CreateIndex(name: "ix_purchases_number", table: "purchases", column: "number", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_purchases_number", table: "purchases");
            migrationBuilder.DropIndex(name: "ix_purchases_branch_id_branch_sequence", table: "purchases");
            migrationBuilder.DropIndex(name: "ix_sales_number", table: "sales");
            migrationBuilder.DropIndex(name: "ix_sales_branch_id_branch_sequence", table: "sales");

            foreach (var table in NumberedTables)
            {
                migrationBuilder.DropColumn(name: "number", table: table);
                migrationBuilder.DropColumn(name: "branch_sequence", table: table);
            }

            // The counters describe documents that no longer carry a number.
            migrationBuilder.Sql("UPDATE branches SET sale_sequence = 0, purchase_sequence = 0;");
        }
    }
}
