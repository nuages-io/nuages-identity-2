using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer.Migrations
{
    public partial class AddINdexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserHandleBase64",
                table: "Fido2Credentials",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptorIdBase64",
                table: "Fido2Credentials",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_DescriptorIdBase64",
                table: "Fido2Credentials",
                column: "DescriptorIdBase64",
                unique: true,
                filter: "[DescriptorIdBase64] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserHandleBase64",
                table: "Fido2Credentials",
                column: "UserHandleBase64");

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserIdBase64_DescriptorIdBase64",
                table: "Fido2Credentials",
                columns: new[] { "UserIdBase64", "DescriptorIdBase64" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fido2Credentials_DescriptorIdBase64",
                table: "Fido2Credentials");

            migrationBuilder.DropIndex(
                name: "IX_Fido2Credentials_UserHandleBase64",
                table: "Fido2Credentials");

            migrationBuilder.DropIndex(
                name: "IX_Fido2Credentials_UserIdBase64_DescriptorIdBase64",
                table: "Fido2Credentials");

            migrationBuilder.AlterColumn<string>(
                name: "UserHandleBase64",
                table: "Fido2Credentials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptorIdBase64",
                table: "Fido2Credentials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
