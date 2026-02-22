using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InfosecAcademyBudgetManagement.Migrations
{
    /// <inheritdoc />
    public partial class BudgetingAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CostItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentContentType",
                table: "CostItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentName",
                table: "CostItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "CostItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DocumentSize",
                table: "CostItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BudgetPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BudgetPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetPlanId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_BudgetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetLines_BudgetPlans_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "BudgetPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_BudgetPlanId_CategoryId_Month",
                table: "BudgetLines",
                columns: new[] { "BudgetPlanId", "CategoryId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_CategoryId",
                table: "BudgetLines",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetLines");

            migrationBuilder.DropTable(
                name: "BudgetPlans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CostItems");

            migrationBuilder.DropColumn(
                name: "DocumentContentType",
                table: "CostItems");

            migrationBuilder.DropColumn(
                name: "DocumentName",
                table: "CostItems");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "CostItems");

            migrationBuilder.DropColumn(
                name: "DocumentSize",
                table: "CostItems");
        }
    }
}

