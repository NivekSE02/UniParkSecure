using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniParkSecure.Migrations
{
    /// <inheritdoc />
    public partial class FixTriggersAndColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Column alterations (wrap in TRY to avoid second-run failures if already applied)
            migrationBuilder.Sql(@"
BEGIN TRY
    ALTER TABLE [Registros] ALTER COLUMN [UserId] nvarchar(450) NULL;
END TRY BEGIN CATCH END CATCH;
BEGIN TRY
    ALTER TABLE [Registros] ALTER COLUMN [SectorId] int NULL;
END TRY BEGIN CATCH END CATCH;
BEGIN TRY
    ALTER TABLE [Registros] ALTER COLUMN [Placa] nvarchar(max) NULL;
END TRY BEGIN CATCH END CATCH;
BEGIN TRY
    ALTER TABLE [Registros] ALTER COLUMN [FotoPath] nvarchar(max) NULL;
END TRY BEGIN CATCH END CATCH;
BEGIN TRY
    ALTER TABLE [Registros] ALTER COLUMN [DUI] nvarchar(max) NULL;
END TRY BEGIN CATCH END CATCH;", suppressTransaction: true);

            // Limpiar datos inválidos antes de crear FK de Sector
            migrationBuilder.Sql(@"UPDATE Registros SET SectorId = NULL WHERE SectorId IS NOT NULL AND SectorId NOT IN (SELECT Id FROM Sectores);");
            // Limpiar datos inválidos para UserId (si hubiera huérfanos)
            migrationBuilder.Sql(@"UPDATE Registros SET UserId = NULL WHERE UserId IS NOT NULL AND UserId NOT IN (SELECT Id FROM AspNetUsers);");

            // Crear índices si no existen
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Registros_SectorId')
                                    CREATE INDEX [IX_Registros_SectorId] ON [Registros] ([SectorId]);");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Registros_UserId')
                                    CREATE INDEX [IX_Registros_UserId] ON [Registros] ([UserId]);");

            // Agregar FK a AspNetUsers si no existe
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Registros_AspNetUsers_UserId')
                                    ALTER TABLE [Registros] ADD CONSTRAINT [FK_Registros_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]);");

            // Agregar FK a Sectores si no existe
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Registros_Sectores_SectorId')
                                    ALTER TABLE [Registros] ADD CONSTRAINT [FK_Registros_Sectores_SectorId] FOREIGN KEY ([SectorId]) REFERENCES [Sectores] ([Id]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys if exist
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Registros_AspNetUsers_UserId')
                                    ALTER TABLE [Registros] DROP CONSTRAINT [FK_Registros_AspNetUsers_UserId];");
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Registros_Sectores_SectorId')
                                    ALTER TABLE [Registros] DROP CONSTRAINT [FK_Registros_Sectores_SectorId];");

            // Drop indexes if exist
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Registros_SectorId')
                                    DROP INDEX [IX_Registros_SectorId] ON [Registros];");
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Registros_UserId')
                                    DROP INDEX [IX_Registros_UserId] ON [Registros];");

            // Revert columns (best effort)
            migrationBuilder.Sql(@"BEGIN TRY ALTER TABLE [Registros] ALTER COLUMN [UserId] nvarchar(max) NOT NULL; END TRY BEGIN CATCH END CATCH;");
            migrationBuilder.Sql(@"BEGIN TRY ALTER TABLE [Registros] ALTER COLUMN [SectorId] int NOT NULL; END TRY BEGIN CATCH END CATCH;");
            migrationBuilder.Sql(@"BEGIN TRY ALTER TABLE [Registros] ALTER COLUMN [Placa] nvarchar(max) NOT NULL; END TRY BEGIN CATCH END CATCH;");
            migrationBuilder.Sql(@"BEGIN TRY ALTER TABLE [Registros] ALTER COLUMN [FotoPath] nvarchar(max) NOT NULL; END TRY BEGIN CATCH END CATCH;");
            migrationBuilder.Sql(@"BEGIN TRY ALTER TABLE [Registros] ALTER COLUMN [DUI] nvarchar(max) NOT NULL; END TRY BEGIN CATCH END CATCH;");
        }
    }
}
