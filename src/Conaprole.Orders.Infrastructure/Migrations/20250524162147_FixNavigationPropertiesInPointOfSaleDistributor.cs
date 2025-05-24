using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conaprole.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationPropertiesInPointOfSaleDistributor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_point_of_sale_distributor_distributor_distributor_id1",
                table: "point_of_sale_distributor");

            migrationBuilder.DropForeignKey(
                name: "fk_point_of_sale_distributor_point_of_sale_point_of_sale_id1",
                table: "point_of_sale_distributor");

            migrationBuilder.DropIndex(
                name: "ix_point_of_sale_distributor_distributor_id1",
                table: "point_of_sale_distributor");

            migrationBuilder.DropIndex(
                name: "ix_point_of_sale_distributor_point_of_sale_id1",
                table: "point_of_sale_distributor");

            migrationBuilder.DropColumn(
                name: "distributor_id1",
                table: "point_of_sale_distributor");

            migrationBuilder.DropColumn(
                name: "point_of_sale_id1",
                table: "point_of_sale_distributor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "distributor_id1",
                table: "point_of_sale_distributor",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "point_of_sale_id1",
                table: "point_of_sale_distributor",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_point_of_sale_distributor_distributor_id1",
                table: "point_of_sale_distributor",
                column: "distributor_id1");

            migrationBuilder.CreateIndex(
                name: "ix_point_of_sale_distributor_point_of_sale_id1",
                table: "point_of_sale_distributor",
                column: "point_of_sale_id1");

            migrationBuilder.AddForeignKey(
                name: "fk_point_of_sale_distributor_distributor_distributor_id1",
                table: "point_of_sale_distributor",
                column: "distributor_id1",
                principalTable: "distributor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_point_of_sale_distributor_point_of_sale_point_of_sale_id1",
                table: "point_of_sale_distributor",
                column: "point_of_sale_id1",
                principalTable: "point_of_sale",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
