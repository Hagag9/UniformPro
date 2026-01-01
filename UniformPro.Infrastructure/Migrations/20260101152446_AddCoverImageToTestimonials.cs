using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniformPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverImageToTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImage",
                table: "Testimonials",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "Testimonials");
        }
    }
}
