using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class Invoice
{
    public int Id { get; set; }

    public DateTime? DateCreated { get; set; }

    public decimal? GrandTotal { get; set; }

    public decimal? CustomerPayment { get; set; }

    public decimal? ChangeMoney { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
