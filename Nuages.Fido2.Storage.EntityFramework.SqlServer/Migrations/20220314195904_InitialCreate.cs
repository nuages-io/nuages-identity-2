using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fido2Credentials",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserIdBase64 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                    CredType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescriptorIdBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptorType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptorTransports = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserHandleBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptorJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fido2Credentials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserIdBase64",
                table: "Fido2Credentials",
                column: "UserIdBase64");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fido2Credentials");
        }
    }
}
