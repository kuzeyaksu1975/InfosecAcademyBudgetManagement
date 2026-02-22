using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InfosecAcademyBudgetManagement.migration
{
    /// <inheritdoc />
    public partial class AddEmailAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailAccountSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SmtpHost = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SmtpPort = table.Column<int>(type: "integer", nullable: false),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccountSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAccountSettings");
        }
    }
}
