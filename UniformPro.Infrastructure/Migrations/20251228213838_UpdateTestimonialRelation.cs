using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniformPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestimonialRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Testimonials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "PortfolioId",
                table: "Testimonials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_PortfolioId",
                table: "Testimonials",
                column: "PortfolioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Testimonials_Portfolios_PortfolioId",
                table: "Testimonials",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Testimonials_Portfolios_PortfolioId",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_PortfolioId",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "PortfolioId",
                table: "Testimonials");

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Testimonials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
