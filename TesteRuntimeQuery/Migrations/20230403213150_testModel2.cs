using Microsoft.EntityFrameworkCore.Migrations;

namespace TesteRuntimeQuery.Migrations
{
    public partial class testModel2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Model2Id",
                table: "TestModel",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestModel2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SuperProp1 = table.Column<int>(type: "int", nullable: false),
                    SuperProp2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestModel2", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestModel_Model2Id",
                table: "TestModel",
                column: "Model2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestModel_TestModel2_Model2Id",
                table: "TestModel",
                column: "Model2Id",
                principalTable: "TestModel2",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestModel_TestModel2_Model2Id",
                table: "TestModel");

            migrationBuilder.DropTable(
                name: "TestModel2");

            migrationBuilder.DropIndex(
                name: "IX_TestModel_Model2Id",
                table: "TestModel");

            migrationBuilder.DropColumn(
                name: "Model2Id",
                table: "TestModel");
        }
    }
}
