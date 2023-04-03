using Microsoft.EntityFrameworkCore.Migrations;

namespace TesteRuntimeQuery.Migrations
{
    public partial class teste2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestChildModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prop1 = table.Column<int>(type: "int", nullable: false),
                    Prop2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestChildModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestChildModel_TestModel_TestModelId",
                        column: x => x.TestModelId,
                        principalTable: "TestModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestChildModel_TestModelId",
                table: "TestChildModel",
                column: "TestModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestChildModel");
        }
    }
}
