using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class OutputInfo
{
    public int Id { get; set; }

    public int IdOutput { get; set; }

    public string IdObject { get; set; } = null!;

    public int IdCustomer { get; set; }

    public int? Count { get; set; }

    public string? Status { get; set; }

    public virtual Customer IdCustomerNavigation { get; set; } = null!;

    public virtual Object IdObjectNavigation { get; set; } = null!;

    public virtual Output IdOutputNavigation { get; set; } = null!;
}
