using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourWear_backend.Migrations
{
    /// <inheritdoc />
    public partial class EditableWears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditableObject",
                table: "PublishedWears",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditableObject",
                table: "PublishedWears");
        }
    }
}
