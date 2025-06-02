using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conaprole.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributorIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "distributor_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_distributor_id",
                table: "users",
                column: "distributor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_distributor_distributor_id",
                table: "users",
                column: "distributor_id",
                principalTable: "distributor",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_distributor_distributor_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_distributor_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "distributor_id",
                table: "users");
        }
    }
}
