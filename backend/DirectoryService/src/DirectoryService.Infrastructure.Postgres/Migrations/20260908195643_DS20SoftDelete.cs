using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DS20SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "positions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "positions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "departments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_positions_soft_delete",
                table: "positions",
                columns: new[] { "is_deleted", "deleted_at" },
                filter: "is_deleted = TRUE AND deleted_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_locations_soft_delete",
                table: "locations",
                columns: new[] { "is_deleted", "deleted_at" },
                filter: "is_deleted = TRUE AND deleted_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_departments_soft_delete",
                table: "departments",
                columns: new[] { "is_deleted", "deleted_at" },
                filter: "is_deleted = TRUE AND deleted_at IS NOT NULL");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_locations_name_upper\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_positions_name_upper\";");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_locations_name_upper\" ON locations (UPPER(name)) WHERE is_deleted = FALSE;");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_positions_name_upper\" ON positions (UPPER(name)) WHERE is_deleted = FALSE;");

            migrationBuilder.Sql(
                "CREATE VIEW active_locations AS SELECT * FROM locations WHERE is_deleted = FALSE;");
            migrationBuilder.Sql(
                "CREATE VIEW active_departments AS SELECT * FROM departments WHERE is_deleted = FALSE;");
            migrationBuilder.Sql(
                "CREATE VIEW active_positions AS SELECT * FROM positions WHERE is_deleted = FALSE;");
            migrationBuilder.Sql("""
                CREATE VIEW active_department_locations AS
                SELECT dl.*
                FROM department_locations AS dl
                INNER JOIN active_departments AS d ON d.id = dl.department_id
                INNER JOIN active_locations AS l ON l.id = dl.location_id;
                """);
            migrationBuilder.Sql("""
                CREATE VIEW active_department_positions AS
                SELECT dp.*
                FROM department_positions AS dp
                INNER JOIN active_departments AS d ON d.id = dp.department_id
                INNER JOIN active_positions AS p ON p.id = dp.position_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS active_department_locations;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS active_department_positions;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS active_locations;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS active_departments;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS active_positions;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_locations_name_upper\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_positions_name_upper\";");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_locations_name_upper\" ON locations (UPPER(name));");
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_positions_name_upper\" ON positions (UPPER(name));");

            migrationBuilder.DropIndex(
                name: "IX_positions_soft_delete",
                table: "positions");

            migrationBuilder.DropIndex(
                name: "IX_locations_soft_delete",
                table: "locations");

            migrationBuilder.DropIndex(
                name: "IX_departments_soft_delete",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "positions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "locations");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "departments");
        }
    }
}
#pragma warning restore CA1861
