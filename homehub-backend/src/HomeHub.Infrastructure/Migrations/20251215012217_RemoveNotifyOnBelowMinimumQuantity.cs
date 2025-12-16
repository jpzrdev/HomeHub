using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotifyOnBelowMinimumQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyOnBelowMinimumQuantity",
                table: "InventoryItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnBelowMinimumQuantity",
                table: "InventoryItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
