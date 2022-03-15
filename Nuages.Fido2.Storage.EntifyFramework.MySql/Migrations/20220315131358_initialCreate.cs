using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nuages.Fido2.Storage.EntifyFramework.MySql.Migrations
{
    public partial class initialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fido2Credentials",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<byte[]>(type: "longblob", nullable: true),
                    UserIdBase64 = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublicKey = table.Column<byte[]>(type: "longblob", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "longblob", nullable: true),
                    SignatureCounter = table.Column<uint>(type: "int unsigned", nullable: false),
                    CredType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AaGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DescriptorIdBase64 = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptorType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptorTransports = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserHandleBase64 = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescriptorJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fido2Credentials", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_DescriptorIdBase64",
                table: "Fido2Credentials",
                column: "DescriptorIdBase64",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserHandleBase64",
                table: "Fido2Credentials",
                column: "UserHandleBase64");

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserIdBase64",
                table: "Fido2Credentials",
                column: "UserIdBase64");

            migrationBuilder.CreateIndex(
                name: "IX_Fido2Credentials_UserIdBase64_DescriptorIdBase64",
                table: "Fido2Credentials",
                columns: new[] { "UserIdBase64", "DescriptorIdBase64" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fido2Credentials");
        }
    }
}
