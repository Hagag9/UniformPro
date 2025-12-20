using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniformPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSocialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwitterUrl",
                table: "SiteSettings",
                newName: "YoutubeUrl");

            migrationBuilder.RenameColumn(
                name: "LinkedInUrl",
                table: "SiteSettings",
                newName: "TikTokUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "YoutubeUrl",
                table: "SiteSettings",
                newName: "TwitterUrl");

            migrationBuilder.RenameColumn(
                name: "TikTokUrl",
                table: "SiteSettings",
                newName: "LinkedInUrl");
        }
    }
}
