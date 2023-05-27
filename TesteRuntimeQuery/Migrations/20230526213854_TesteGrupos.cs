using Microsoft.EntityFrameworkCore.Migrations;

namespace TesteRuntimeQuery.Migrations
{
    public partial class TesteGrupos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupTestingClass",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyA = table.Column<int>(type: "int", nullable: false),
                    Propertyb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Propertyc = table.Column<bool>(type: "bit", nullable: false),
                    Propertyd = table.Column<int>(type: "int", nullable: false),
                    Propertye = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTestingClass", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupTestingClass");
        }
    }
}
