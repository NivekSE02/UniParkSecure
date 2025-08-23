using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniParkSecure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatriculaToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "AspNetUsers");
        }
    }
}
