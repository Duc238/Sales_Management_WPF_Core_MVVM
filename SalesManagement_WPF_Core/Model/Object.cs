using SalesManagement_WPF_Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

namespace SalesManagement_WPF_Core.Model;

public partial class Object:BaseViewModel
{
    private string _Id;
    public string Id { get=>_Id; set { _Id = value;OnPropertyChanged(); } }
    private string? _DisplayName;
    public string? DisplayName { get=>_DisplayName; set { _DisplayName = value;OnPropertyChanged(); } }
    private int _IdUnit;
    public int IdUnit { get=>_IdUnit; set { _IdUnit = value;OnPropertyChanged(); } }

    private int _IdSuplier;
    public int IdSuplier { get=>_IdSuplier; set { _IdSuplier = value;OnPropertyChanged(); } }

    private int? _PercentPrice;
    public int? PercentPrice { get=>_PercentPrice; set { _PercentPrice = value;OnPropertyChanged(); } }

    private string? _BarCode;
    public string? BarCode { get=>_BarCode; set { _BarCode = value;OnPropertyChanged(); } }

    private decimal? _InputPrice;
    public decimal? InputPrice { get=>_InputPrice; set { _InputPrice = value;OnPropertyChanged(); } }

    private decimal? _OutputPrice;
    public decimal? OutputPrice { get=>_OutputPrice; set { _OutputPrice = value;OnPropertyChanged(); } }
    private Suplier _IdSuplierNavigation;
    public virtual Suplier IdSuplierNavigation { get=>_IdSuplierNavigation; set { _IdSuplierNavigation = value;OnPropertyChanged(); } }
    private Unit _IdUnitNavigation;
    public virtual Unit IdUnitNavigation { get=>_IdUnitNavigation; set { _IdUnitNavigation = value;OnPropertyChanged(); } }
    private ICollection<OutputInfo> _OutputInfos;
    public virtual ICollection<OutputInfo> OutputInfos { get=>_OutputInfos; set { _OutputInfos = value;OnPropertyChanged(); } }
    private bool _IsDeleted;
    public bool IsDeleted { get=>_IsDeleted; set { _IsDeleted = value;OnPropertyChanged(); } }
}
