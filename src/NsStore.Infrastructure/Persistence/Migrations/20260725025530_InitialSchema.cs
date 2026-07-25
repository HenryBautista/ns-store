using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NsStore.Domain.Enums;

#nullable disable

namespace NsStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:client_type", "company,individual")
                .Annotation("Npgsql:Enum:invoice_type", "with_invoice,without_invoice")
                .Annotation("Npgsql:Enum:movement_type", "adjustment,purchase,sale")
                .Annotation("Npgsql:Enum:order_status", "cancelled,delivered,pending")
                .Annotation("Npgsql:Enum:payment_status", "credit,paid")
                .Annotation("Npgsql:Enum:user_role", "admin,seller");

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<ClientType>(type: "client_type", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    mother_last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ci = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    nit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    city = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trademarks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trademarks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    first_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    mother_last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    role = table.Column<UserRole>(type: "user_role", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "warranty_terms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_warranty_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_date = table.Column<DateOnly>(type: "date", nullable: false),
                    client_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_type = table.Column<InvoiceType>(type: "invoice_type", nullable: false),
                    payment_status = table.Column<PaymentStatus>(type: "payment_status", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_paid = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales", x => x.id);
                    table.CheckConstraint("ck_sales_total_amount_non_negative", "total_amount >= 0");
                    table.CheckConstraint("ck_sales_total_paid_within_total", "total_paid >= 0 AND total_paid <= total_amount");
                    table.CheckConstraint("ck_sales_total_quantity_positive", "total_quantity > 0");
                    table.ForeignKey(
                        name: "fk_sales_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_type = table.Column<InvoiceType>(type: "invoice_type", nullable: false),
                    payment_status = table.Column<PaymentStatus>(type: "payment_status", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchases", x => x.id);
                    table.CheckConstraint("ck_purchases_total_amount_non_negative", "total_amount >= 0");
                    table.CheckConstraint("ck_purchases_total_quantity_positive", "total_quantity > 0");
                    table.ForeignKey(
                        name: "fk_purchases_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_date = table.Column<DateOnly>(type: "date", nullable: false),
                    client_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    product_description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    advance_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    status = table.Column<OrderStatus>(type: "order_status", nullable: false),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.CheckConstraint("ck_orders_advance_within_price", "advance_amount >= 0 AND advance_amount <= price");
                    table.CheckConstraint("ck_orders_price_non_negative", "price >= 0");
                    table.ForeignKey(
                        name: "fk_orders_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quote_date = table.Column<DateOnly>(type: "date", nullable: false),
                    client_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    detail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                    table.CheckConstraint("ck_quotes_price_non_negative", "price >= 0");
                    table.ForeignKey(
                        name: "fk_quotes_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    part_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    trademark_id = table.Column<long>(type: "bigint", nullable: true),
                    category_id = table.Column<long>(type: "bigint", nullable: true),
                    warranty_term_id = table.Column<long>(type: "bigint", nullable: true),
                    price_with_invoice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    price_without_invoice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_products_price_with_invoice_non_negative", "price_with_invoice >= 0");
                    table.CheckConstraint("ck_products_price_without_invoice_non_negative", "price_without_invoice >= 0");
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_trademarks_trademark_id",
                        column: x => x.trademark_id,
                        principalTable: "trademarks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_warranty_terms_warranty_term_id",
                        column: x => x.warranty_term_id,
                        principalTable: "warranty_terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("ck_payments_amount_positive", "amount > 0");
                    table.ForeignKey(
                        name: "fk_payments_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    movement_type = table.Column<MovementType>(type: "movement_type", nullable: false),
                    quantity_delta = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    reference_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reference_id = table.Column<long>(type: "bigint", nullable: true),
                    notes = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventory_movements", x => x.id);
                    table.CheckConstraint("ck_inventory_movements_quantity_delta_not_zero", "quantity_delta <> 0");
                    table.ForeignKey(
                        name: "fk_inventory_movements_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_items", x => x.id);
                    table.CheckConstraint("ck_purchase_items_quantity_positive", "quantity > 0");
                    table.CheckConstraint("ck_purchase_items_unit_price_non_negative", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_purchase_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_items_purchases_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_items", x => x.id);
                    table.CheckConstraint("ck_sale_items_quantity_positive", "quantity > 0");
                    table.CheckConstraint("ck_sale_items_unit_price_non_negative", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_sale_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sale_items_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_levels",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_levels", x => x.product_id);
                    table.CheckConstraint("ck_stock_levels_quantity_non_negative", "quantity >= 0");
                    table.ForeignKey(
                        name: "fk_stock_levels_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_clients_last_name",
                table: "clients",
                column: "last_name");

            migrationBuilder.CreateIndex(
                name: "ix_clients_name",
                table: "clients",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_clients_nit",
                table: "clients",
                column: "nit");

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_product_id_created_at",
                table: "inventory_movements",
                columns: new[] { "product_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_inventory_movements_reference_type_reference_id",
                table: "inventory_movements",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_date",
                table: "orders",
                column: "order_date");

            migrationBuilder.CreateIndex(
                name: "ix_orders_owner_id",
                table: "orders",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_sale_id_payment_date",
                table: "payments",
                columns: new[] { "sale_id", "payment_date" });

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_name",
                table: "products",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_products_part_number",
                table: "products",
                column: "part_number");

            migrationBuilder.CreateIndex(
                name: "ix_products_trademark_id",
                table: "products",
                column: "trademark_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_warranty_term_id",
                table: "products",
                column: "warranty_term_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_product_id",
                table: "purchase_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_purchase_id",
                table: "purchase_items",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_purchase_date",
                table: "purchases",
                column: "purchase_date");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_supplier_id",
                table: "purchases",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_owner_id",
                table: "quotes",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_quote_date",
                table: "quotes",
                column: "quote_date");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family_id",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_items_product_id",
                table: "sale_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_items_sale_id",
                table: "sale_items",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_client_id",
                table: "sales",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_payment_status_sale_date",
                table: "sales",
                columns: new[] { "payment_status", "sale_date" });

            migrationBuilder.CreateIndex(
                name: "ix_sales_sale_date",
                table: "sales",
                column: "sale_date");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name",
                table: "suppliers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_trademarks_name",
                table: "trademarks",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "ix_warranty_terms_description",
                table: "warranty_terms",
                column: "description");

            // Case-insensitive uniqueness over active rows only. EF Core cannot express an index
            // on an expression, so these partial unique indexes are declared directly.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_users_username_active ON users (lower(username)) WHERE deleted_at IS NULL;");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_trademarks_name_active ON trademarks (lower(name)) WHERE deleted_at IS NULL;");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_categories_name_active ON categories (lower(name)) WHERE deleted_at IS NULL;");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_suppliers_name_active ON suppliers (lower(name)) WHERE deleted_at IS NULL;");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX ux_warranty_terms_description_active ON warranty_terms (lower(description)) WHERE deleted_at IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_users_username_active;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_trademarks_name_active;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_categories_name_active;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_suppliers_name_active;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_warranty_terms_description_active;");

            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropTable(
                name: "inventory_movements");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "purchase_items");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "sale_items");

            migrationBuilder.DropTable(
                name: "stock_levels");

            migrationBuilder.DropTable(
                name: "purchases");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "sales");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "trademarks");

            migrationBuilder.DropTable(
                name: "warranty_terms");
        }
    }
}
