public record CollectionsSummaryResponse(
    int RemindersSent,
    int RemindersFailed,
    int RemindersSkipped,
    DateTime? LastRunAt
);

public record OperationsSummaryResponse(
    int TotalPickupGuides,
    int PendingPickupGuides,
    int CompletedPickupGuides,
    decimal TotalAmount
);

public record PaymentMethodBreakdownItem(string Method, decimal Amount, int Count);

public record ReceptionIncomeSummaryResponse(
    decimal MonthIncome,
    decimal MonthAdvance,
    decimal PendingBalance,
    int GuideCount,
    List<PaymentMethodBreakdownItem> PaymentMethodBreakdown
);

public record DeliverySummaryResponse(
    int DeliveryCount,
    decimal DeliveryAmount,
    int ReceptionCount,
    decimal ReceptionAmount,
    int DeliveryPending,
    int DeliveryCompleted
);

public record ExpenseBreakdownItem(string Category, decimal Amount, decimal PercentOfTotal);

// "Esto gano, esto gasto, esto me queda": TotalIncome = facturado (Invoicing + ReceptionIncome).
// TotalExpenses = Gastos + Compras + Planilla (estimada). Ver nota de PendingCollectionRatio:
// cuanto de lo facturado todavia no se cobra, como señal de riesgo de caja independiente de la rentabilidad.
public record BusinessSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetResult,
    decimal MarginPercent,
    bool IsProfitable,
    List<ExpenseBreakdownItem> ExpenseBreakdown,
    decimal TotalPendingToCollect,
    decimal PendingCollectionRatio
);

public record ManagementDashboardResponse(
    InvoiceDashboardResponse Invoicing,
    CollectionsSummaryResponse Collections,
    OperationsSummaryResponse Operations,
    DeliverySummaryResponse Deliveries,
    ReceptionIncomeSummaryResponse ReceptionIncome,
    ExpenseDashboardSummaryResponse Expenses,
    PurchaseDashboardSummaryResponseDto Purchases,
    HrDashboardSummaryResponseDto Hr,
    BusinessSummaryResponse BusinessSummary
);
