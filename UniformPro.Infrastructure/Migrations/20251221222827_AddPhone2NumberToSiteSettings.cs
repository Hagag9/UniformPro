using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniformPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhone2NumberToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone2Number",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone2Number",
                table: "SiteSettings");
        }
    }
}
