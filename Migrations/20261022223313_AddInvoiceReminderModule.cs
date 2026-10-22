using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceReminderModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DriverName",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "DriverDocument",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "RecipientMode",
                table: "ScheduledEmailNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSuppressed",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReminderSuppressedReason",
                table: "Invoices",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RemindersEnabled",
                table: "CustomerBillingProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RemindersSuppressedReason",
                table: "CustomerBillingProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceReminderLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledEmailNotificationId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SkipReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    InvoiceCount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceReminderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceReminderLogs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceReminderLogs_ScheduledEmailNotifications_ScheduledEmailNotificationId",
                        column: x => x.ScheduledEmailNotificationId,
                        principalTable: "ScheduledEmailNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceReminderLogDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceReminderLogId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DaysOverdue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceReminderLogDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceReminderLogDetails_InvoiceReminderLogs_InvoiceReminderLogId",
                        column: x => x.InvoiceReminderLogId,
                        principalTable: "InvoiceReminderLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceReminderLogDetails_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceReminderLogDetails_InvoiceId",
                table: "InvoiceReminderLogDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceReminderLogDetails_InvoiceReminderLogId",
                table: "InvoiceReminderLogDetails",
                column: "InvoiceReminderLogId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceReminderLogs_CustomerId_RunAt",
                table: "InvoiceReminderLogs",
                columns: new[] { "CustomerId", "RunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceReminderLogs_ScheduledEmailNotificationId",
                table: "InvoiceReminderLogs",
                column: "ScheduledEmailNotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceReminderLogDetails");

            migrationBuilder.DropTable(
                name: "InvoiceReminderLogs");

            migrationBuilder.DropColumn(
                name: "RecipientMode",
                table: "ScheduledEmailNotifications");

            migrationBuilder.DropColumn(
                name: "ReminderSuppressed",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReminderSuppressedReason",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RemindersEnabled",
                table: "CustomerBillingProfiles");

            migrationBuilder.DropColumn(
                name: "RemindersSuppressedReason",
                table: "CustomerBillingProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "DriverName",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DriverDocument",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
