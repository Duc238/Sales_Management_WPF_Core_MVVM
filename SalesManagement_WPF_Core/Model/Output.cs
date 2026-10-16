using System;
using System.Collections.Generic;

namespace SalesManagement_WPF_Core.Model;

public partial class Output
{
    public int Id { get; set; }

    public DateTime DateOutput { get; set; }

    public virtual ICollection<OutputInfo> OutputInfos { get; set; } = new List<OutputInfo>();
}
