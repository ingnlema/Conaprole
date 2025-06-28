using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conaprole.Orders.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPermissionsAndForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert missing users:read permission with ID 1 only if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT INTO permissions (id, name)
                SELECT 1, 'users:read'
                WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE id = 1);
            ");

            // Ensure Registered role (ID 1) has users:read and products:read permissions
            // Insert only if they don't already exist
            migrationBuilder.Sql(@"
                INSERT INTO role_permissions (permission_id, role_id)
                SELECT 1, 1
                WHERE NOT EXISTS (SELECT 1 FROM role_permissions WHERE permission_id = 1 AND role_id = 1);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO role_permissions (permission_id, role_id)
                SELECT 7, 1
                WHERE NOT EXISTS (SELECT 1 FROM role_permissions WHERE permission_id = 7 AND role_id = 1);
            ");

            // Update foreign key constraint for orders.user_id to ON DELETE SET NULL
            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Remove role permissions if they exist
            migrationBuilder.Sql(@"
                DELETE FROM role_permissions 
                WHERE permission_id = 1 AND role_id = 1;
            ");

            migrationBuilder.Sql(@"
                DELETE FROM role_permissions 
                WHERE permission_id = 7 AND role_id = 1;
            ");

            // Remove users:read permission if it exists
            migrationBuilder.Sql(@"
                DELETE FROM permissions WHERE id = 1;
            ");
        }
    }
}
