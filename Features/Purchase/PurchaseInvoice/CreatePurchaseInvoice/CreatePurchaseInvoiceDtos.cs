public record PurchaseInvoiceRequestDto
{
    public int SupplierId { get; set; }
    public string InvoiceType { get; set; } = string.Empty;
    public string InvoiceSerie { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceIssueDate { get; set; } = DateTime.Now;
    public DateTime InvoiceExpirationDate { get; set; } = DateTime.Now;
    public string TypePayment { get; set; } = string.Empty; // contado / credito    
    public int DaysCredit { get; set; } = 0; // Number of days for credit payment
    public string PaymentMethod { get; set; } = string.Empty; // e.g., "Cash", "Credit Card"
    public string CurrencyId { get; set; } = "";    
    public decimal ExchangeRate { get; set; } = 1.0m;     
    public string Comments { get; set; } = "";    
    public decimal Subtotal { get; set; }    
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public ICollection<PurchaseInvoiceDetailsRequestDto>? PurchaseInvoiceDetails { get; set; }

    /// <summary>
    /// Almacén donde ingresan los productos. Si se informa, se registra el movimiento de inventario
    /// "Ingreso por compra" para los productos que controlan stock.
    /// </summary>
    public int? WarehouseId { get; set; }
}

public record PurchaseInvoiceResponseDto
{
    public int Id { get; set; }
}

public record PurchaseInvoiceDetailsRequestDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }    
}


public record PurchaseInvoiceDetailsDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public string ProductName { get; set; } = string.Empty;
}