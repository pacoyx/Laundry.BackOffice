using _24dex_backend_comercial.Features.PrinterHub;
using Microsoft.AspNetCore.Builder;

public static class RouteExtensions
{
    public static void MapRoutes(this WebApplication app)
    {
        app.MapLogin();
        app.MapExpenseBox();
        app.MapHealth();
        app.MapBranchSales();
        app.MapCustomer();
        app.MapCustomers();
        app.MapCatService();
        app.MapProdService();
        app.MapNumbersDocument();
        app.MapWorkShift();
        app.MapWorkGuideMain();
        app.MapCustomerGarmentHistory();
        app.MapCashBox();
        app.MapReport();
        app.MapLocationClothes();
        app.MapUser();
        app.MapCompany();
        app.MapEmailAccounts();
        app.MapBanks();
        app.MapBankAccountTypes();
        app.MapCompanyBankAccounts();
        app.MapTicket();
        app.MapCollectionGuide();
        app.MapClothingWorkerEndpoints();
        app.MapClothingItemEndpoints();
        app.MapUnitMeasurement();
        app.MapCategoryProd();
        app.MapProduct();
        app.MapSupplier();
        app.MapPurchaseInvoice();
        app.MapPurchaseReports();
        app.MapAccountsPayable();
        app.MapPurchaseDashboard();
        app.MapPrinter();
        app.MapBrachSalesUser();
        app.MapPrintingAdmin();
        app.MapDiscoverPrinters();
        app.MapGetPrintJob();
        app.MapCompletePrintJob();
        app.MapErrorPrintJob();
        app.MapGetPendingPrintQueues();
        app.MapCancelPrintQueue();
        app.MapReprintPrintQueue();
        app.MapWorkerNode();
        app.MapInvoices();
        app.MapInvoiceDashboard();
        app.MapAccountsReceivable();
        app.MapCollectionSheets();
        app.MapPaymentMethod();
        app.MapTypeDocument();
        app.MapSneakers();
        app.MapInspections();

        app.MapArea();
        app.MapPosition();
        app.MapEmployee();
        app.MapEmployeeDocument();
        app.MapAttendance();
        app.MapIncident();
        app.MapExtraHour();
        app.MapLoan();
        app.MapPayrollPeriod();
        app.MapPayroll();
        app.MapHrDashboard();
        app.MapHrReports();

        app.MapExpenseCategories();
        app.MapExpenses();
        app.MapExpenseDashboard();
        app.MapExpenseReports();

        app.MapRoles();
        app.MapPermissions();
        app.MapUserRoles();
        app.MapUserAccessSchedule();
        app.MapAuth();

        app.MapCustomerPriceLists();
        app.MapPickupGuides();
        app.MapVehicles();
        app.MapCustomerPortal();
        app.MapCustomerUsersAdmin();
        app.MapPickupMobile();
        app.MapScheduledEmailNotifications();
        app.MapInvoiceReminders();
        app.MapManagementDashboard();
        app.MapWebOrders();
        app.MapGuiasRemision();
    }
}