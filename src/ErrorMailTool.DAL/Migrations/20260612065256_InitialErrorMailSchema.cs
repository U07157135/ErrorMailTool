using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErrorMailTool.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialErrorMailSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorMails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    From = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasContentFile = table.Column<bool>(type: "bit", nullable: false),
                    IsContentComplete = table.Column<bool>(type: "bit", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorMails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorMailAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErrorMailId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FullPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorMailAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorMailAttachments_ErrorMails_ErrorMailId",
                        column: x => x.ErrorMailId,
                        principalTable: "ErrorMails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorMailAttachments_ErrorMailId",
                table: "ErrorMailAttachments",
                column: "ErrorMailId");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorMails_FolderPath",
                table: "ErrorMails",
                column: "FolderPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErrorMails_OccurredAt",
                table: "ErrorMails",
                column: "OccurredAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorMailAttachments");

            migrationBuilder.DropTable(
                name: "ErrorMails");
        }
    }
}
