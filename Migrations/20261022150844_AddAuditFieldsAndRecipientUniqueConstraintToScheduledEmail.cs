using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsAndRecipientUniqueConstraintToScheduledEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledEmailRecipients_ScheduledEmailNotificationId_Email_RecipientType",
                table: "ScheduledEmailRecipients");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ScheduledEmailNotifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmailRecipients_ScheduledEmailNotificationId_Email",
                table: "ScheduledEmailRecipients",
                columns: new[] { "ScheduledEmailNotificationId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduledEmailRecipients_ScheduledEmailNotificationId_Email",
                table: "ScheduledEmailRecipients");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ScheduledEmailNotifications");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ScheduledEmailNotifications");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEmailRecipients_ScheduledEmailNotificationId_Email_RecipientType",
                table: "ScheduledEmailRecipients",
                columns: new[] { "ScheduledEmailNotificationId", "Email", "RecipientType" },
                unique: true);
        }
    }
}
