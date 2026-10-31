using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedAssetsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixedAssetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultDepreciationRatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DefaultUsefulLifeYears = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    UsefulLifeYearsOverride = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DisposalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposalReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssets_BranchSales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "BranchSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_FixedAssetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FixedAssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssets_Users_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetMaintenances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixedAssetId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetMaintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssetMaintenances_FixedAssets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "FixedAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetReassignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixedAssetId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousBranchId = table.Column<int>(type: "int", nullable: true),
                    NewBranchId = table.Column<int>(type: "int", nullable: false),
                    PreviousResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    NewResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetReassignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssetReassignments_BranchSales_NewBranchId",
                        column: x => x.NewBranchId,
                        principalTable: "BranchSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetReassignments_BranchSales_PreviousBranchId",
                        column: x => x.PreviousBranchId,
                        principalTable: "BranchSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetReassignments_FixedAssets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "FixedAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FixedAssetReassignments_Users_NewResponsibleUserId",
                        column: x => x.NewResponsibleUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FixedAssetReassignments_Users_PreviousResponsibleUserId",
                        column: x => x.PreviousResponsibleUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FixedAssetMaintenanceAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixedAssetMaintenanceId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedAssetMaintenanceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedAssetMaintenanceAttachments_FixedAssetMaintenances_FixedAssetMaintenanceId",
                        column: x => x.FixedAssetMaintenanceId,
                        principalTable: "FixedAssetMaintenances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetCategories_Name",
                table: "FixedAssetCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetMaintenanceAttachments_FixedAssetMaintenanceId",
                table: "FixedAssetMaintenanceAttachments",
                column: "FixedAssetMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetMaintenances_FixedAssetId",
                table: "FixedAssetMaintenances",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetReassignments_FixedAssetId",
                table: "FixedAssetReassignments",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetReassignments_NewBranchId",
                table: "FixedAssetReassignments",
                column: "NewBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetReassignments_NewResponsibleUserId",
                table: "FixedAssetReassignments",
                column: "NewResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetReassignments_PreviousBranchId",
                table: "FixedAssetReassignments",
                column: "PreviousBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssetReassignments_PreviousResponsibleUserId",
                table: "FixedAssetReassignments",
                column: "PreviousResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_BranchId",
                table: "FixedAssets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_CategoryId",
                table: "FixedAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_Code",
                table: "FixedAssets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_PurchaseInvoiceId",
                table: "FixedAssets",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_ResponsibleUserId",
                table: "FixedAssets",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FixedAssets_SupplierId",
                table: "FixedAssets",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixedAssetMaintenanceAttachments");

            migrationBuilder.DropTable(
                name: "FixedAssetReassignments");

            migrationBuilder.DropTable(
                name: "FixedAssetMaintenances");

            migrationBuilder.DropTable(
                name: "FixedAssets");

            migrationBuilder.DropTable(
                name: "FixedAssetCategories");
        }
    }
}
