using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniParkSecure.Migrations
{
    public partial class RemoveTriggersAndNullableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar triggers si existen
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.trg_EntradaParqueo', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_EntradaParqueo;");
            migrationBuilder.Sql(@"IF OBJECT_ID('dbo.trg_SalidaParqueo', 'TR') IS NOT NULL DROP TRIGGER dbo.trg_SalidaParqueo;");

            // Ajustar nullabilidad de columnas opcionales
            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Registros",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FotoPath",
                table: "Registros",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // DUI definido como requerido en tu respuesta, lo dejamos NOT NULL.
            // Si actualmente es nullable en el modelo, deberías actualizar el modelo a no-null.

            // UserId y SectorId deben ser NOT NULL según tu decisión.
            // Aseguramos que no existan nulos antes de aplicar restricciones (por seguridad).
            migrationBuilder.Sql("UPDATE Registros SET UserId = '' WHERE UserId IS NULL;");
            migrationBuilder.Sql("UPDATE Registros SET SectorId = 0 WHERE SectorId IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir nullabilidad
            migrationBuilder.AlterColumn<string>(
                name: "Placa",
                table: "Registros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FotoPath",
                table: "Registros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // (No recreamos triggers automáticamente en Down para evitar efectos secundarios.)
        }
    }
}
