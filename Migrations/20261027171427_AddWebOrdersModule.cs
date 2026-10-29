using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddWebOrdersModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EmployeeId",
                table: "ItemPhotos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "ItemPhotos",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Origin",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsCentral",
                table: "BranchSales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WebCustomerAccounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCustomerAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebCustomerAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebCustomerOtps",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCustomerOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkGuideDetailChangeLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkGuideMainId = table.Column<int>(type: "int", nullable: false),
                    WorkGuideDetailId = table.Column<int>(type: "int", nullable: true),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    OldQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NewQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGuideDetailChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkGuideDetailChangeLogs_ProdServices_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ProdServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkGuideDetailChangeLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkGuideDetailChangeLogs_WorkGuideDetails_WorkGuideDetailId",
                        column: x => x.WorkGuideDetailId,
                        principalTable: "WorkGuideDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkGuideDetailChangeLogs_WorkGuideMains_WorkGuideMainId",
                        column: x => x.WorkGuideMainId,
                        principalTable: "WorkGuideMains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZonePriceLists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonePriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZonePriceLists_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebOrders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ZonePriceListId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedPickupDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledPickupDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    WorkGuideMainId = table.Column<int>(type: "int", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PickupObservation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_WorkGuideMains_WorkGuideMainId",
                        column: x => x.WorkGuideMainId,
                        principalTable: "WorkGuideMains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrders_ZonePriceLists_ZonePriceListId",
                        column: x => x.ZonePriceListId,
                        principalTable: "ZonePriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZonePriceListDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZonePriceListId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonePriceListDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZonePriceListDetails_ProdServices_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ProdServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZonePriceListDetails_ZonePriceLists_ZonePriceListId",
                        column: x => x.ZonePriceListId,
                        principalTable: "ZonePriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebOrderDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebOrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebOrderDetails_ProdServices_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ProdServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WebOrderDetails_WebOrders_WebOrderId",
                        column: x => x.WebOrderId,
                        principalTable: "WebOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_EmployeeId",
                table: "ItemPhotos",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DistrictId",
                table: "Customers",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSales_IsCentral_Single",
                table: "BranchSales",
                column: "IsCentral",
                unique: true,
                filter: "[IsCentral] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ZoneId",
                table: "Districts",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_WebCustomerAccounts_CustomerId",
                table: "WebCustomerAccounts",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebCustomerAccounts_Phone",
                table: "WebCustomerAccounts",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebCustomerOtps_Phone_IsUsed",
                table: "WebCustomerOtps",
                columns: new[] { "Phone", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_WebOrderDetails_ServiceItemId",
                table: "WebOrderDetails",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrderDetails_WebOrderId",
                table: "WebOrderDetails",
                column: "WebOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_ConfirmedByUserId",
                table: "WebOrders",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_CustomerId_Status",
                table: "WebOrders",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_DistrictId",
                table: "WebOrders",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_EmployeeId_Status",
                table: "WebOrders",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_OrderNumber",
                table: "WebOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_VehicleId",
                table: "WebOrders",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_WorkGuideMainId",
                table: "WebOrders",
                column: "WorkGuideMainId");

            migrationBuilder.CreateIndex(
                name: "IX_WebOrders_ZonePriceListId",
                table: "WebOrders",
                column: "ZonePriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGuideDetailChangeLogs_ServiceItemId",
                table: "WorkGuideDetailChangeLogs",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGuideDetailChangeLogs_UserId",
                table: "WorkGuideDetailChangeLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGuideDetailChangeLogs_WorkGuideDetailId",
                table: "WorkGuideDetailChangeLogs",
                column: "WorkGuideDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkGuideDetailChangeLogs_WorkGuideMainId",
                table: "WorkGuideDetailChangeLogs",
                column: "WorkGuideMainId");

            migrationBuilder.CreateIndex(
                name: "IX_ZonePriceListDetails_ServiceItemId",
                table: "ZonePriceListDetails",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ZonePriceListDetails_ZonePriceListId_ServiceItemId",
                table: "ZonePriceListDetails",
                columns: new[] { "ZonePriceListId", "ServiceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZonePriceLists_ZoneId_ActiveOnly",
                table: "ZonePriceLists",
                column: "ZoneId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ZonePriceLists_ZoneId_Version",
                table: "ZonePriceLists",
                columns: new[] { "ZoneId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_Name",
                table: "Zones",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Districts_DistrictId",
                table: "Customers",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPhotos_Employees_EmployeeId",
                table: "ItemPhotos",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Districts_DistrictId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemPhotos_Employees_EmployeeId",
                table: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "WebCustomerAccounts");

            migrationBuilder.DropTable(
                name: "WebCustomerOtps");

            migrationBuilder.DropTable(
                name: "WebOrderDetails");

            migrationBuilder.DropTable(
                name: "WorkGuideDetailChangeLogs");

            migrationBuilder.DropTable(
                name: "ZonePriceListDetails");

            migrationBuilder.DropTable(
                name: "WebOrders");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "ZonePriceLists");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_ItemPhotos_EmployeeId",
                table: "ItemPhotos");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DistrictId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_BranchSales_IsCentral_Single",
                table: "BranchSales");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "ItemPhotos");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "ItemPhotos");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsCentral",
                table: "BranchSales");
        }
    }
}
