using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InfosecAcademyBudgetManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "CostItems");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "CostItems",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Updatedby = table.Column<string>(type: "text", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "Name", "UpdatedAt", "Updatedby" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Fixed", new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Utilities", new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Office", new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Travel", new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, "Other", new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostItems_CategoryId",
                table: "CostItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CostItems_Categories_CategoryId",
                table: "CostItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostItems_Categories_CategoryId",
                table: "CostItems");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_CostItems_CategoryId",
                table: "CostItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CostItems");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "CostItems",
                type: "text",
                nullable: true);
        }
    }
}

