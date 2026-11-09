using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddGarmentIntakeTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyWorkers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyWorkers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyWorkers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GarmentIntakeTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedByCustomerUserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    VoidedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoidedByCustomerUserId = table.Column<long>(type: "bigint", nullable: true),
                    PickupGuideId = table.Column<long>(type: "bigint", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentIntakeTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTickets_CustomerUsers_CreatedByCustomerUserId",
                        column: x => x.CreatedByCustomerUserId,
                        principalTable: "CustomerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTickets_CustomerUsers_VoidedByCustomerUserId",
                        column: x => x.VoidedByCustomerUserId,
                        principalTable: "CustomerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTickets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTickets_PickupGuides_PickupGuideId",
                        column: x => x.PickupGuideId,
                        principalTable: "PickupGuides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GarmentIntakeTicketDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentIntakeTicketDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTicketDetails_CompanyWorkers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "CompanyWorkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTicketDetails_GarmentIntakeTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "GarmentIntakeTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTicketDetails_ProdServices_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "ProdServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GarmentIntakeTicketPhotos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByCustomerUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GarmentIntakeTicketPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTicketPhotos_CustomerUsers_CreatedByCustomerUserId",
                        column: x => x.CreatedByCustomerUserId,
                        principalTable: "CustomerUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GarmentIntakeTicketPhotos_GarmentIntakeTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "GarmentIntakeTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyWorkers_CustomerId_Code",
                table: "CompanyWorkers",
                columns: new[] { "CustomerId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTicketDetails_ServiceItemId",
                table: "GarmentIntakeTicketDetails",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTicketDetails_TicketId",
                table: "GarmentIntakeTicketDetails",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTicketDetails_WorkerId",
                table: "GarmentIntakeTicketDetails",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTicketPhotos_CreatedByCustomerUserId",
                table: "GarmentIntakeTicketPhotos",
                column: "CreatedByCustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTicketPhotos_TicketId",
                table: "GarmentIntakeTicketPhotos",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTickets_CreatedByCustomerUserId",
                table: "GarmentIntakeTickets",
                column: "CreatedByCustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTickets_CustomerId_Status",
                table: "GarmentIntakeTickets",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTickets_PickupGuideId",
                table: "GarmentIntakeTickets",
                column: "PickupGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTickets_TicketNumber",
                table: "GarmentIntakeTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarmentIntakeTickets_VoidedByCustomerUserId",
                table: "GarmentIntakeTickets",
                column: "VoidedByCustomerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GarmentIntakeTicketDetails");

            migrationBuilder.DropTable(
                name: "GarmentIntakeTicketPhotos");

            migrationBuilder.DropTable(
                name: "CompanyWorkers");

            migrationBuilder.DropTable(
                name: "GarmentIntakeTickets");
        }
    }
}
