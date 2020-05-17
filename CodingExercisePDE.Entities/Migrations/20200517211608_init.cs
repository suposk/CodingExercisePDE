using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodingExercisePDE.Entities.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RandomNumbers",
                columns: table => new
                {
                    RandomNumberId = table.Column<Guid>(nullable: false),
                    MessageId = table.Column<Guid>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RandomNumbers", x => x.RandomNumberId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RandomNumbers");
        }
    }
}
