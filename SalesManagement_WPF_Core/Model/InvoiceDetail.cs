using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class InvoiceDetail
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public string? DisplayName { get; set; }

    public decimal? OutputPrice { get; set; }

    public int? Quantity { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
