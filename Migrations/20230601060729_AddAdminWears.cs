using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourWear_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminWears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "PublishedWears",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "PublishedWears");
        }
    }
}
