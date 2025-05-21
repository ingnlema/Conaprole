using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conaprole.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class POSandDistributorFeatureMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_categories");

            migrationBuilder.DropColumn(
                name: "distributor",
                table: "orders");

            migrationBuilder.AddColumn<int>(
                name: "category",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "point_of_sale_id",
                table: "orders");

            migrationBuilder.AddColumn<Guid>(
                name: "point_of_sale_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty); 

            migrationBuilder.AddColumn<Guid>(
                name: "distributor_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "distributor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    supported_categories = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_distributor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "point_of_sale",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_point_of_sale", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "point_of_sale_distributor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    point_of_sale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    distributor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_point_of_sale_distributor", x => x.id);
                    table.ForeignKey(
                        name: "fk_point_of_sale_distributor_distributor_distributor_id",
                        column: x => x.distributor_id,
                        principalTable: "distributor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_point_of_sale_distributor_point_of_sale_point_of_sale_id",
                        column: x => x.point_of_sale_id,
                        principalTable: "point_of_sale",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_distributor_id",
                table: "orders",
                column: "distributor_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_point_of_sale_id",
                table: "orders",
                column: "point_of_sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_point_of_sale_distributor_distributor_id",
                table: "point_of_sale_distributor",
                column: "distributor_id");

            migrationBuilder.CreateIndex(
                name: "ix_point_of_sale_distributor_point_of_sale_id",
                table: "point_of_sale_distributor",
                column: "point_of_sale_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Distributor",
                table: "orders",
                column: "distributor_id",
                principalTable: "distributor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PointOfSale",
                table: "orders",
                column: "point_of_sale_id",
                principalTable: "point_of_sale",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Distributor",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PointOfSale",
                table: "orders");

            migrationBuilder.DropTable(
                name: "point_of_sale_distributor");

            migrationBuilder.DropTable(
                name: "distributor");

            migrationBuilder.DropTable(
                name: "point_of_sale");

            migrationBuilder.DropIndex(
                name: "ix_orders_distributor_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_point_of_sale_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "category",
                table: "products");

            migrationBuilder.DropColumn(
                name: "distributor_id",
                table: "orders");

            migrationBuilder.AlterColumn<string>(
                name: "point_of_sale_id",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "distributor",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_categories", x => new { x.product_id, x.category });
                    table.ForeignKey(
                        name: "fk_product_categories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
