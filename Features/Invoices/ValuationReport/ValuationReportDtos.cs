public sealed class ValuationReportFilter
{
    public int CustomerId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}

public sealed class ValuationReportGuideRowDto
{
    public long Id { get; set; }

    public string GuideNumber { get; set; } = string.Empty;

    public DateTime PickupDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public bool HasPhysicalGuidePhoto { get; set; }
}

public sealed class ValuationReportServiceColumnDto
{
    public int ServiceItemId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public bool IsPricedByWeight { get; set; }
}

public sealed class ValuationReportPivotRowDto
{
    public DateTime Date { get; set; }

    public Dictionary<int, decimal> QuantitiesByServiceItemId { get; set; } = new();

    public decimal RowTotal { get; set; }
}

public sealed class ValuationReportPivotDto
{
    public List<ValuationReportServiceColumnDto> Services { get; set; } = new();

    public List<ValuationReportPivotRowDto> Rows { get; set; } = new();

    public Dictionary<int, decimal> ColumnTotals { get; set; } = new();

    public decimal GrandTotal { get; set; }
}

public sealed class ValuationReportResponse
{
    public string CustomerName { get; set; } = string.Empty;

    public List<ValuationReportGuideRowDto> Guides { get; set; } = new();

    public ValuationReportPivotDto Pivot { get; set; } = new();

    public int PhotoCount { get; set; }
}
