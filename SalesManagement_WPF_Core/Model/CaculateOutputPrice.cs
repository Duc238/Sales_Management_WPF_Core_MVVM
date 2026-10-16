using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class CaculateOutputPrice
{
    public string Id { get; set; } = null!;

    public string IdObject { get; set; } = null!;

    public decimal? InputPrice { get; set; }

    public int PercentPrice { get; set; }

    public decimal? OutputPrice { get; set; }
}
