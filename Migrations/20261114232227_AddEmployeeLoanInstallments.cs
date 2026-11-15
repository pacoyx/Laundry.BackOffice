using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _24dex_backend_comercial.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLoanInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "EmployeeLoans",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "EmployeeLoanInstallments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeLoanId = table.Column<long>(type: "bigint", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PayrollId = table.Column<long>(type: "bigint", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLoanInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLoanInstallments_EmployeeLoans_EmployeeLoanId",
                        column: x => x.EmployeeLoanId,
                        principalTable: "EmployeeLoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLoanInstallments_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoanInstallments_EmployeeLoanId_InstallmentNumber",
                table: "EmployeeLoanInstallments",
                columns: new[] { "EmployeeLoanId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoanInstallments_PayrollId",
                table: "EmployeeLoanInstallments",
                column: "PayrollId");

            // Préstamos existentes: se registran como 1 cuota. Los vigentes quedan con la cuota pendiente por su saldo;
            // los cancelados quedan con la cuota pagada (sin vínculo a planilla, porque antes no se registraba).
            migrationBuilder.Sql(@"
INSERT INTO EmployeeLoanInstallments (EmployeeLoanId, InstallmentNumber, Amount, IsPaid, PayrollId, PaidAt)
SELECT l.Id,
       1,
       CASE WHEN l.IsClosed = 1 OR l.Balance <= 0 THEN l.Amount ELSE l.Balance END,
       CASE WHEN l.IsClosed = 1 OR l.Balance <= 0 THEN 1 ELSE 0 END,
       NULL,
       NULL
FROM EmployeeLoans l
WHERE NOT EXISTS (SELECT 1 FROM EmployeeLoanInstallments i WHERE i.EmployeeLoanId = l.Id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLoanInstallments");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "EmployeeLoans");
        }
    }
}
