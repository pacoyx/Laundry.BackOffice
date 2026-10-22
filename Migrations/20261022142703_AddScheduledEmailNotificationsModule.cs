using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledEmailNotificationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledEmailNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DriverDocument = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SignatureName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SignaturePosition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SignaturePhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEmailNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEmailExecutions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledEmailNotificationId = table.Column<int>(type: "int", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEmailExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEmailExecutions_ScheduledEmailNotifications_ScheduledEmailNotificationId",
                        column: x => x.ScheduledEmailNotificationId,
                        principalTable: "ScheduledEmailNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEmailRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledEmailNotificationId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEmailRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledEmailRecipients_ScheduledEmailNotifications_ScheduledEmailNotificationId",
                        column: x => x.ScheduledEmailNotificationId,
                        principalTable: "ScheduledEmailNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmailExecutions_ScheduledEmailNotificationId_ExecutedAt",
                table: "ScheduledEmailExecutions",
                columns: new[] { "ScheduledEmailNotificationId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmailNotifications_Code",
                table: "ScheduledEmailNotifications",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmailRecipients_ScheduledEmailNotificationId_Email_RecipientType",
                table: "ScheduledEmailRecipients",
                columns: new[] { "ScheduledEmailNotificationId", "Email", "RecipientType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledEmailExecutions");

            migrationBuilder.DropTable(
                name: "ScheduledEmailRecipients");

            migrationBuilder.DropTable(
                name: "ScheduledEmailNotifications");
        }
    }
}
