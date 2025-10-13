using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDealPipelineAndStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DealPipelines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealPipelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DealStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ExpectedDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealStages_DealPipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "DealPipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DealAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExpectedCloseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PipelineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    StageStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StageDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_DealPipelines_PipelineId",
                        column: x => x.PipelineId,
                        principalTable: "DealPipelines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_DealStages_CurrentStageId",
                        column: x => x.CurrentStageId,
                        principalTable: "DealStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deals_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DealHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DealId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeInStage = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealHistory_DealStages_FromStageId",
                        column: x => x.FromStageId,
                        principalTable: "DealStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealHistory_DealStages_ToStageId",
                        column: x => x.ToStageId,
                        principalTable: "DealStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealHistory_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealHistory_ChangedAt",
                table: "DealHistory",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DealHistory_DealId",
                table: "DealHistory",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_DealHistory_FromStageId",
                table: "DealHistory",
                column: "FromStageId");

            migrationBuilder.CreateIndex(
                name: "IX_DealHistory_ToStageId",
                table: "DealHistory",
                column: "ToStageId");

            migrationBuilder.CreateIndex(
                name: "IX_DealPipelines_CreatedAt",
                table: "DealPipelines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DealPipelines_IsActive",
                table: "DealPipelines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_ClientId",
                table: "Deals",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatedAt",
                table: "Deals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CurrentStageId",
                table: "Deals",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_ExpectedCloseDate",
                table: "Deals",
                column: "ExpectedCloseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_IsActive",
                table: "Deals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_PipelineId",
                table: "Deals",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_PropertyId",
                table: "Deals",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_RequestId",
                table: "Deals",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_StageDeadline",
                table: "Deals",
                column: "StageDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_DealStages_Order",
                table: "DealStages",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_DealStages_PipelineId",
                table: "DealStages",
                column: "PipelineId");

            migrationBuilder.CreateIndex(
                name: "IX_DealStages_PipelineId_Order",
                table: "DealStages",
                columns: new[] { "PipelineId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealHistory");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "DealStages");

            migrationBuilder.DropTable(
                name: "DealPipelines");
        }
    }
}
