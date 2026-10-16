using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class InputInfo
{
    public string Id { get; set; } = null!;

    public string IdObject { get; set; } = null!;

    public string IdInput { get; set; } = null!;

    public int? Count { get; set; }

    public decimal? InputPrice { get; set; }

    public decimal? OutputPrice { get; set; }

    public string? Status { get; set; }

    public virtual Input IdInputNavigation { get; set; } = null!;

    public virtual Object IdObjectNavigation { get; set; } = null!;
}
