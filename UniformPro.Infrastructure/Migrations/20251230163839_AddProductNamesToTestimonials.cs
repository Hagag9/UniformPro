using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniformPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNamesToTestimonials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductNameAr",
                table: "Testimonials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductNameEn",
                table: "Testimonials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductNameAr",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "ProductNameEn",
                table: "Testimonials");
        }
    }
}
