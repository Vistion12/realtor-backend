using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddClientPersonalAccountAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountLogin",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentGivenAt",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentIpAddress",
                table: "Clients",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentToPersonalData",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPersonalAccount",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccountActive",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TemporaryPassword",
                table: "Clients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DealId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "general"),
                    UploadedBy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiredUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClientEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDocuments_Clients_ClientEntityId",
                        column: x => x.ClientEntityId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClientDocuments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientDocuments_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccountLogin",
                table: "Clients",
                column: "AccountLogin");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_HasPersonalAccount",
                table: "Clients",
                column: "HasPersonalAccount");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsAccountActive",
                table: "Clients",
                column: "IsAccountActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_Category",
                table: "ClientDocuments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_ClientEntityId",
                table: "ClientDocuments",
                column: "ClientEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_ClientId",
                table: "ClientDocuments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_DealId",
                table: "ClientDocuments",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_IsRequired",
                table: "ClientDocuments",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_RequiredUntil",
                table: "ClientDocuments",
                column: "RequiredUntil");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_UploadedAt",
                table: "ClientDocuments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_UploadedBy",
                table: "ClientDocuments",
                column: "UploadedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Clients_AccountLogin",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_HasPersonalAccount",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IsAccountActive",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AccountLogin",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ConsentGivenAt",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ConsentIpAddress",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ConsentToPersonalData",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HasPersonalAccount",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsAccountActive",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TemporaryPassword",
                table: "Clients");
        }
    }
}
