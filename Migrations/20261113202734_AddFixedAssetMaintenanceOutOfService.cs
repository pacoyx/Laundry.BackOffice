using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddFixedAssetMaintenanceOutOfService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "FixedAssetMaintenances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosingNotes",
                table: "FixedAssetMaintenances",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedEndDate",
                table: "FixedAssetMaintenances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutOfServiceReason",
                table: "FixedAssetMaintenances",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "FixedAssetMaintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PutsAssetOutOfService",
                table: "FixedAssetMaintenances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ServiceProvider",
                table: "FixedAssetMaintenances",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "ClosingNotes",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "EstimatedEndDate",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "OutOfServiceReason",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "PutsAssetOutOfService",
                table: "FixedAssetMaintenances");

            migrationBuilder.DropColumn(
                name: "ServiceProvider",
                table: "FixedAssetMaintenances");
        }
    }
}
