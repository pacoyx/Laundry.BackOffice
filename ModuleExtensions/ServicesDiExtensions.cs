using _24dex_backend_comercial.Features.PrinterHub;
using _24dex_backend_comercial.Features.WorkGuideMain.CreateWorkGuideMain;

public static class ServicesDiExtensions
{

    public static void AddServicesDi(this IServiceCollection services)
    {
        services.AddScoped<IScheduledEmailService, ScheduledEmailService>();
        services.AddScoped<IApplicationSeeder, ScheduledEmailNotificationSeeder>();
        services.AddScoped<IOverdueInvoiceReminderService, OverdueInvoiceReminderService>();
        services.AddScoped<IApplicationSeeder, InvoiceReminderNotificationSeeder>();

        services.AddScoped<IGetServicesAccessFastService, GetServicesAccessFastService>();
        services.AddScoped<ICreateUserService, CreateUserService>();
        services.AddScoped<IDeleteUserService, DeleteUserService>();
        services.AddScoped<IGetUsersService, GetUsersService>();
        services.AddScoped<IGetUserService, GetUserService>();
        services.AddScoped<IUpdateUserService, UpdateUserService>();

        services.AddScoped<ICreateCompanyService, CreateCompanyService>();
        services.AddScoped<IDeleteCompanyService, DeleteCompanyService>();
        services.AddScoped<IGetCompaniesService, GetCompaniesService>();
        services.AddScoped<IGetCompanyService, GetCompanyService>();
        services.AddScoped<IUpdateCompanyService, UpdateCompanyService>();
        services.AddScoped<IGetCompanyByUserService, GetCompanyByUserService>();

        services.AddScoped<IEncryptService, EncryptService>();

        services.AddScoped<ICreateUnitMeasurementService, CreateUnitMeasurementService>();
        services.AddScoped<IGetUnitMeasurementsService, GetUnitMeasurementsService>();
        services.AddScoped<IUpdateUnitMeasurementService, UpdateUnitMeasurementService>();
        services.AddScoped<IDeleteUnitMeasurementService, DeleteUnitMeasurementService>();
        services.AddScoped<IGetCategoriesProdService, GetCategoriesProdService>();
        services.AddScoped<ICreateCategoryProdService, CreateCategoryProdService>();
        services.AddScoped<IUpdateCategoryProdService, UpdateCategoryProdService>();
        services.AddScoped<IDeleteCategoryProdService, DeleteCategoryProdService>();
        
        services.AddScoped<IGetProductsService, GetProductsService>();
        services.AddScoped<ICreateProductService, CreateProductService>();
        services.AddScoped<IUpdateProductService, UpdateProductService>();

        services.AddScoped<ICreateSupplierService, CreateSupplierService>();
        services.AddScoped<IUpdateSupplierService, UpdateSupplierService>();
        services.AddScoped<IGetSupplierService, GetSupplierService>();
        
        services.AddScoped<ICreatePurchaseInvoiceService, CreatePurchaseInvoiceService>();
        services.AddScoped<IGetPuchasesInvoiceService, GetPuchasesInvoiceService>();
        services.AddScoped<IGetInvoicesByParamsService, GetInvoicesByParamsService>();
        services.AddScoped<IRegisterPurchasePaymentService, RegisterPurchasePaymentService>();
        services.AddScoped<IGetPurchasePaymentsService, GetPurchasePaymentsService>();
        services.AddScoped<ICancelPurchaseInvoiceService, CancelPurchaseInvoiceService>();
        
        services.AddScoped<IPrintNotifier, SignalRPrintNotifier>();
        services.AddSingleton<PrinterRegistry>();

        services.AddScoped<ISalesRegisterReportService, SalesRegisterReportService>();
        services.AddScoped<IValuationReportService, ValuationReportService>();
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<ICompanyProfileService, CompanyProfileService>();
        services.AddScoped<IPurchaseRegisterReportService, PurchaseRegisterReportService>();

        services.AddScoped<ICreateAreaService, CreateAreaService>();
        services.AddScoped<IGetAreasService, GetAreasService>();
        services.AddScoped<IUpdateAreaService, UpdateAreaService>();
        services.AddScoped<IDeleteAreaService, DeleteAreaService>();

        services.AddScoped<ICreatePositionService, CreatePositionService>();
        services.AddScoped<IGetPositionsService, GetPositionsService>();
        services.AddScoped<IUpdatePositionService, UpdatePositionService>();
        services.AddScoped<IDeletePositionService, DeletePositionService>();

        services.AddScoped<ICreateEmployeeService, CreateEmployeeService>();
        services.AddScoped<IGetEmployeesService, GetEmployeesService>();
        services.AddScoped<IGetEmployeeService, GetEmployeeService>();
        services.AddScoped<IUpdateEmployeeService, UpdateEmployeeService>();
        services.AddScoped<ITerminateEmployeeService, TerminateEmployeeService>();

        services.AddScoped<ICreateEmployeeDocumentService, CreateEmployeeDocumentService>();
        services.AddScoped<IGetEmployeeDocumentsService, GetEmployeeDocumentsService>();
        services.AddScoped<IDeleteEmployeeDocumentService, DeleteEmployeeDocumentService>();
        services.AddScoped<IGetEmployeeDocumentTypesService, GetEmployeeDocumentTypesService>();

        services.AddScoped<IRegisterAttendanceService, RegisterAttendanceService>();
        services.AddScoped<IGetAttendancesService, GetAttendancesService>();
        services.AddScoped<IRegisterIncidentService, RegisterIncidentService>();
        services.AddScoped<IGetIncidentsService, GetIncidentsService>();
        services.AddScoped<IRegisterExtraHourService, RegisterExtraHourService>();
        services.AddScoped<IGetExtraHoursService, GetExtraHoursService>();
        services.AddScoped<IApproveExtraHourService, ApproveExtraHourService>();

        services.AddScoped<IRegisterLoanService, RegisterLoanService>();
        services.AddScoped<IGetLoansService, GetLoansService>();

        services.AddScoped<ICreatePayrollPeriodService, CreatePayrollPeriodService>();
        services.AddScoped<IGetPayrollPeriodsService, GetPayrollPeriodsService>();
        services.AddScoped<IGeneratePayrollService, GeneratePayrollService>();
        services.AddScoped<IGetPayrollService, GetPayrollService>();

        services.AddScoped<IGetPayrollReportService, GetPayrollReportService>();
        services.AddScoped<IGetLoanReportService, GetLoanReportService>();

        services.AddScoped<IExpenseReportService, ExpenseReportService>();

        services.AddScoped<IImageCompressor, SkiaImageCompressor>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<ISymmetricEncryptionService, AesSymmetricEncryptionService>();
        services.AddScoped<IPickupGuideReceiptPdfService, PickupGuideReceiptPdfService>();

        services.AddScoped<IApplicationSeeder, AreaSeeder>();
        services.AddScoped<IApplicationSeeder, PositionSeeder>();
        services.AddScoped<IApplicationSeeder, EmployeeDocumentTypeSeeder>();
        services.AddScoped<IApplicationSeeder, SneakerBrandSeeder>();
        services.AddScoped<IApplicationSeeder, ExpenseCategorySeeder>();
        services.AddScoped<IApplicationSeeder, PaymentMethodSeeder>();
        services.AddScoped<IApplicationSeeder, EmailAccountSeeder>();
        services.AddScoped<IApplicationSeeder, BankAccountTypeSeeder>();

        services.AddScoped<IApplicationSeeder, PermissionSeeder>();
        services.AddScoped<IApplicationSeeder, RoleSeeder>();
        services.AddScoped<IApplicationSeeder, RolePermissionSeeder>();
        services.AddScoped<IApplicationSeeder, UserRoleBootstrapSeeder>();
        services.AddScoped<IApplicationSeeder, PickupMobileTestUserSeeder>();

        services.AddScoped<IApplicationSeeder, CustomerPermissionSeeder>();
        services.AddScoped<IApplicationSeeder, CustomerRoleSeeder>();
        services.AddScoped<IApplicationSeeder, CustomerRolePermissionSeeder>();
    }
}