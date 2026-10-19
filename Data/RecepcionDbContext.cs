using DexterCompany.Models;
using Microsoft.EntityFrameworkCore;


public class RecepcionDbContext : DbContext
{
    public RecepcionDbContext(DbContextOptions<RecepcionDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserDocument> UserDocuments { get; set; }
    public DbSet<NumbersDocument> NumbersDocuments { get; set; }
    public DbSet<CatService> CatServices { get; set; }
    public DbSet<BranchSales> BranchSales { get; set; }
    public DbSet<TypeDocument> TypeDocuments { get; set; }
    public DbSet<ProdService> ProdServices { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerBillingProfile> CustomerBillingProfiles { get; set; }
    public DbSet<CustomerContact> CustomerContacts { get; set; }
    public DbSet<WorkGuideMain> WorkGuideMains { get; set; }
    public DbSet<WorkGuideDetail> WorkGuideDetails { get; set; }
    public DbSet<ExpenseBox> ExpenseBoxMains { get; set; }
    public DbSet<WorkShift> WorkShifts { get; set; }
    public DbSet<CashBoxMain> CashBoxMains { get; set; }
    public DbSet<CashBoxDetail> CashBoxDetails { get; set; }
    public DbSet<BrachSalesUser> BrachSalesUsers { get; set; }
    public DbSet<AlertMsg> AlertMsgs { get; set; }
    public DbSet<LocationClothes> LocationClothes { get; set; }
    public DbSet<LocationWorkGuide> LocationWorkGuides { get; set; }
    public DbSet<ServiceAccessFast> ServiceAccessFasts { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<EmailAccount> EmailAccounts { get; set; }

    public DbSet<ClothingItem>  ClothingItems { get; set; }
    public DbSet<ClothingObservations>  ClothingObservations { get; set; }
    public DbSet<ClothingWorker>  ClothingWorkers { get; set; }
    public DbSet<CollectionGuide>  CollectionGuides { get; set; }
    public DbSet<CollectionGuideTicket>  CollectionGuideTickets { get; set; }
    public DbSet<ObservationSection>  ObservationSections { get; set; }
    public DbSet<Ticket>  Tickets { get; set; }
    public DbSet<TicketClothe>  TicketClothes { get; set; }
    public DbSet<TypeObservation>  TypeObservations { get; set; }
    
    
    public DbSet<UnitMeasurement> UnitMeasurements { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CategoryProd> CategoryProds { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceDetails> PurchaseInvoiceDetails { get; set; } 
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseInvoicePayment> PurchaseInvoicePayments { get; set; }
    
    
    public DbSet<Printer> Printers { get; set; }
    public DbSet<PrintQueue> PrintQueues { get; set; }
    public DbSet<WorkerNode> WorkerNodes { get; set; }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    public DbSet<InvoicePayment> InvoicePayments { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<CollectionSheet> CollectionSheets { get; set; }

    public DbSet<SneakerBrand> SneakerBrands { get; set; }
    public DbSet<SneakerDetail> SneakerDetails { get; set; }
    public DbSet<ItemInspection> ItemInspections { get; set; }
    public DbSet<ItemPhoto> ItemPhotos { get; set; }

    public DbSet<Area> Areas { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeEmergencyContact> EmployeeEmergencyContacts { get; set; }
    public DbSet<EmployeeBankAccount> EmployeeBankAccounts { get; set; }
    public DbSet<EmployeeSalaryHistory> EmployeeSalaryHistories { get; set; }

    public DbSet<EmployeeDocumentType> EmployeeDocumentTypes { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AttendanceIncident> AttendanceIncidents { get; set; }
    public DbSet<EmployeeExtraHour> EmployeeExtraHours { get; set; }

    public DbSet<EmployeeLoan> EmployeeLoans { get; set; }

    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<PayrollDetail> PayrollDetails { get; set; }

    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<PickupGuide> PickupGuides { get; set; }
    public DbSet<PickupGuideDetail> PickupGuideDetails { get; set; }
    public DbSet<CustomerPriceList> CustomerPriceLists { get; set; }
    public DbSet<CustomerPriceListDetail> CustomerPriceListDetails { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<PickupGuideAttachment> PickupGuideAttachments { get; set; }
    public DbSet<PickupGuideRevision> PickupGuideRevisions { get; set; }
    public DbSet<PickupGuideDetailRevision> PickupGuideDetailRevisions { get; set; }
    public DbSet<PickupGuideRevisionPhoto> PickupGuideRevisionPhotos { get; set; }

    public DbSet<CustomerUser> CustomerUsers { get; set; }
    public DbSet<CustomerRole> CustomerRoles { get; set; }
    public DbSet<CustomerPermission> CustomerPermissions { get; set; }
    public DbSet<CustomerRolePermission> CustomerRolePermissions { get; set; }
    public DbSet<CustomerUserToken> CustomerUserTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BranchSales>()
            .HasMany(x => x.WorkerNodes)
            .WithOne(x => x.Branch)
            .HasForeignKey(x => x.BranchId);

        modelBuilder.Entity<WorkerNode>()
            .HasMany(x => x.Printers)
            .WithOne(x => x.WorkerNode)
            .HasForeignKey(x => x.WorkerNodeId);

        modelBuilder.Entity<PrintQueue>()
            .HasOne(x => x.Printer)
            .WithMany()
            .HasForeignKey(x => x.PrinterId);

        modelBuilder.Entity<WorkGuideMain>()
            .HasMany(x => x.PrintQueues)
            .WithOne(x => x.WorkGuideMain)
            .HasForeignKey(x => x.WorkGuideId);
        
        modelBuilder.Entity<PrintQueue>()
            .HasIndex(x => new
            {
                x.PrinterId,
                x.Status
            });

        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceDetailConfiguration());
        modelBuilder.ApplyConfiguration(new InvoicePaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionSheetConfiguration());
        modelBuilder.ApplyConfiguration(new SneakerBrandConfiguration());
        modelBuilder.ApplyConfiguration(new SneakerDetailConfiguration());
        modelBuilder.ApplyConfiguration(new ItemInspectionConfiguration());
        modelBuilder.ApplyConfiguration(new ItemPhotoConfiguration());
        modelBuilder.ApplyConfiguration(new UserDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerBillingProfileConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerContactConfiguration());

        modelBuilder.ApplyConfiguration(new AreaConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEmergencyContactConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeBankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeSalaryHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDocumentTypeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new AttendanceConfiguration());
        modelBuilder.ApplyConfiguration(new AttendanceIncidentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeExtraHourConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeLoanConfiguration());
        modelBuilder.ApplyConfiguration(new PayrollPeriodConfiguration());
        modelBuilder.ApplyConfiguration(new PayrollConfiguration());
        modelBuilder.ApplyConfiguration(new PayrollDetailConfiguration());

        modelBuilder.ApplyConfiguration(new ExpenseCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseConfiguration());
        modelBuilder.ApplyConfiguration(new ExpenseAttachmentConfiguration());

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());

        modelBuilder.ApplyConfiguration(new PickupGuideConfiguration());
        modelBuilder.ApplyConfiguration(new PickupGuideDetailConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerPriceListConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerPriceListDetailConfiguration());

        modelBuilder.ApplyConfiguration(new CustomerUserConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerRoleConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerRolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerUserTokenConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new PickupGuideAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new PickupGuideRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new PickupGuideDetailRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new PickupGuideRevisionPhotoConfiguration());
        modelBuilder.ApplyConfiguration(new EmailAccountConfiguration());

        modelBuilder.Entity<User>()
            .HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkGuideMain>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        DateTime fechaOperacion = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditableEntity = (BaseAuditableEntity)entry.Entity;
            var currentUser = "system"; // Aquí puedes obtener el usuario actual de tu contexto de autenticación
            

            if (entry.State == EntityState.Added)
            {
                auditableEntity.Created = fechaOperacion;
                auditableEntity.CreatedBy = currentUser;
            }
            else
            {
                Entry(auditableEntity).Property(x => x.Created).IsModified = false;
                Entry(auditableEntity).Property(x => x.CreatedBy).IsModified = false;
                auditableEntity.LastModified = fechaOperacion;
                auditableEntity.LastModifiedBy = currentUser;
            }
        }
    }
}