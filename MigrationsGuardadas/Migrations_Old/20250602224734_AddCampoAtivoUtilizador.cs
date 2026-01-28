using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPrecos.API.Migrations
{
    public partial class AddCampoAtivoUtilizador : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "utilizadores",
                type: "boolean",
                nullable: false,
                defaultValue: true); // <-- ajustado de false para true
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ativo",
                table: "utilizadores");
        }
    }
}
