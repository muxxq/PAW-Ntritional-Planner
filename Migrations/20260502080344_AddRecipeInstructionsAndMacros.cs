using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace paw_np.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeInstructionsAndMacros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Recipes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCarbs",
                table: "Recipes",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFats",
                table: "Recipes",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalProteins",
                table: "Recipes",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalCarbs",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalFats",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TotalProteins",
                table: "Recipes");
        }
    }
}
