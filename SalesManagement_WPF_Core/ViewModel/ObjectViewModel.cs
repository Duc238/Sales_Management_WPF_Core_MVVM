using Azure.Core.GeoJson;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SalesManagement_WPF_Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using Object = SalesManagement_WPF_Core.Model.Object;
using Table = System.Windows.Documents.Table;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class ObjectViewModel : BaseViewModel
    {
        private ObservableCollection<Model.Unit> _Unit;
        public ObservableCollection<Model.Unit> Unit { get => _Unit; set { _Unit = value; OnPropertyChanged(); } }

        private ObservableCollection<Model.Suplier> _Suplier;
        public ObservableCollection<Model.Suplier> Suplier { get => _Suplier; set { _Suplier = value; OnPropertyChanged(); } }
        private ICollectionView _FilterView;
        public ICollectionView FilterView { get=>_FilterView; set { _FilterView = value; OnPropertyChanged(); } }
        // 1. Danh sách món không mã vạch
        private ObservableCollection<Model.Object> _quickPickProducts;
        public ObservableCollection<Model.Object> QuickPickProducts
        {
            get => _quickPickProducts;
            set 
            { 
                _quickPickProducts = value; 
                OnPropertyChanged(); 
            }
        }
        private string _SearchProduct;
        public string SearchProduct 
        { 
            get=>_SearchProduct; 
            set 
            { 
                _SearchProduct = value;
                OnPropertyChanged();
                // Gọi hàm lọc mỗi khi giá trị tìm kiếm thay đổi
                FilterView?.Refresh();
            } 
        }

        private void FilterProductList()
        {
            throw new NotImplementedException();
        }

        private Customer _SelectedCustomer;
        public Customer SelectedCustomer { get=>_SelectedCustomer; set { _SelectedCustomer = value;OnPropertyChanged(); } }

        private Model.Object _SelectedItem;
        public Model.Object SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();

                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                    BarCode = SelectedItem.BarCode;

                    // Fix 1: Match theo ID thay vì gán trực tiếp Navigation Property
                    SelectedUnit = Unit?.FirstOrDefault(x => x.Id == SelectedItem.IdUnit);
                    SelectedSuplier = Suplier?.FirstOrDefault(x => x.Id == SelectedItem.IdSuplier);

                    // Fix 2: Tạm thời gán field riêng để tránh trigger CalculateOutputPrice() đè giá trị
                    _InputPrice = SelectedItem.InputPrice ?? 0;
                    _PercentPrice = SelectedItem.PercentPrice?? 0;
                    _OutputPrice = SelectedItem.OutputPrice ?? 0;

                    // Thông báo UI cập nhật đồng loạt
                    OnPropertyChanged(nameof(InputPrice));
                    OnPropertyChanged(nameof(PercentPrice));
                    OnPropertyChanged(nameof(OutputPrice));
                }
            }
        }
        private Model.Object _lastScannedProduct;
        public Model.Object LastScannedProduct
        {
            get => _lastScannedProduct;
            set { _lastScannedProduct = value; OnPropertyChanged(); }
        }
        private Model.Unit _SelectedUnit;
        public Model.Unit SelectedUnit
        {
            get => _SelectedUnit;
            set
            {
                _SelectedUnit = value;
                OnPropertyChanged();
            }
        }

        private Model.Suplier _SelectedSuplier;
        public Model.Suplier SelectedSuplier
        {
            get => _SelectedSuplier;
            set
            {
                _SelectedSuplier = value;
                OnPropertyChanged();
            }
        }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }

        private string _BarCode;
        public string BarCode { get => _BarCode; set { _BarCode = value; OnPropertyChanged(); } }
        private decimal _InputPrice;
        public decimal InputPrice { get=>_InputPrice; set { _InputPrice = value;OnPropertyChanged();CalculateOutputPrice(); } }
        private decimal _PercentPrice;
        public decimal PercentPrice { get=>_PercentPrice; set { _PercentPrice = value;OnPropertyChanged(); CalculateOutputPrice(); } }
        private decimal _OutputPrice;
        public decimal OutputPrice { get=>_OutputPrice; set { _OutputPrice = value;OnPropertyChanged(); } }
        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand CopyBarCodeCommand { get; set; }
        public ICommand PasteAndScanCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        public ICommand PrintInvoiceCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteProductOrderedCommand { get; set; }
        public ICommand TextChangedCommand { get; set; }
        // 2. Command bấm chọn món
        public ICommand AddQuickProductCommand { get; set; }
        public ICommand ImportExcelCommand { get; set; }
        private ObservableCollection<Model.Object> _List;
        public ObservableCollection<Model.Object> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        public ObjectViewModel()
        {
            List = new ObservableCollection<Model.Object>(DataProvider.Instance.Data.Objects);
            FilterView = CollectionViewSource.GetDefaultView(List);
            if (FilterView != null)
            {
                FilterView.Filter = ProductFilter;
            }
            Unit = new ObservableCollection<Model.Unit>(DataProvider.Instance.Data.Units);
            Suplier = new ObservableCollection<Model.Suplier>(DataProvider.Instance.Data.Supliers);
            // Khởi tạo danh sách giỏ hàng
            CartList = new ObservableCollection<CartItem>();
            CartList.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (CartItem item in e.NewItems)
                    {
                        item.PropertyChanged += (sender, args) =>
                        {
                            OnPropertyChanged(nameof(GrandTotal));
                            OnPropertyChanged(nameof(ChangeMoney)); // Cập nhật tiền thối khi số lượng SP trong giỏ đổi
                        };
                    }
                }

                OnPropertyChanged(nameof(GrandTotal));
                OnPropertyChanged(nameof(ChangeMoney)); // Cập nhật tiền thối khi thêm/xóa SP khỏi giỏ
            };
            // Lệnh khi máy quét gửi phím Enter
            ScanBarCodeCommand = new RelayCommand<object>((p) => true, (p) => OnScanBarCode());
            AddCommand = new RelayCommand<object>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                var displayList = DataProvider.Instance.Data.Objects.Where(x => x.DisplayName == DisplayName);
                if (displayList == null || displayList.Count() != 0)
                    return false;

                return true;
            }
            , (p)=>Add(p));
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => DeleteObject(p));
            ExportExcelCommand = new RelayCommand<object>((p) => true, (p) => ExportExcel(p));
            PrintInvoiceCommand = new RelayCommand<object>((p) => CartList.Any(), (p) => PrintInvoice());
            EditCommand = new RelayCommand<object>((p) =>
            {
                if (SelectedItem == null || SelectedSuplier == null || SelectedUnit == null)
                    return false;

                var displayList = DataProvider.Instance.Data.Objects.Where(x => x.Id == SelectedItem.Id);
                if (displayList != null && displayList.Count() != 0)
                    return true;

                return false;

            }, (p) => Edit(p));
            DeleteProductOrderedCommand = new RelayCommand<object>((p) => true, (p) => DeleteProductOrdered(p));
            //TextChangedCommand=new RelayCommand<object>((p) => true, (p) => TextChanged(p));
            AddQuickProductCommand = new RelayCommand<Model.Object>((p) => p != null, (p) =>
            {
                // Nếu sản phẩm không có BarCode, dùng Tên sản phẩm hoặc Id làm mã tạm để OnScanBarCode nhận diện
                if (!string.IsNullOrEmpty(p.BarCode))
                {
                    ScannedBarCode = p.BarCode;
                }
                else
                {
                    // Gán mã tạm theo Tên để hàm OnScanBarCode() tìm thấy sản phẩm trong List
                    ScannedBarCode = p.DisplayName;
                }

                // Gọi trực tiếp hàm quét mã vạch sẵn có
                OnScanBarCode();
            });
            LoadQuickPickProducts();
        }

        // Hàm lọc các sản phẩm có BarCode trống hoặc null
        public void LoadQuickPickProducts()
        {
            using (var db = new SalesManagementContext())
            {
                // Kiểm tra an toàn x.IsDeleted == true thay vì x.IsDeleted != true
                var objects = DataProvider.Instance.Data.Objects
                    .Where(x => x.IsDeleted != true) // Hoặc: .Where(x => x.IsDeleted == null || x.IsDeleted == false)
                    .ToList();

                List = new ObservableCollection<Model.Object>(objects);

                // Lấy danh sách chọn nhanh món không mã vạch
                var quickList = objects.Where(x => string.IsNullOrWhiteSpace(x.BarCode)).ToList();
                QuickPickProducts = new ObservableCollection<Model.Object>(quickList);
            }
        }
        private bool ProductFilter(object obj)
        {
            // Nếu ô tìm kiếm trống -> Hiển thị tất cả
            if (string.IsNullOrWhiteSpace(SearchProduct))
                return true;

            // Ép kiểu an toàn VÀ kiểm tra Name không bị NULL trong CSDL
            if (obj is Object product && !string.IsNullOrEmpty(product.DisplayName))
            {
                return product.DisplayName.IndexOf(SearchProduct.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return false;
        }

        private void Edit(object p)
        {
            var product = DataProvider.Instance.Data.Objects.FirstOrDefault(x => x.Id == SelectedItem.Id);
            if (product != null)
            {
                product.DisplayName = DisplayName;
                product.BarCode = BarCode;
                product.InputPrice = InputPrice;
                product.PercentPrice = (int?)PercentPrice;
                product.OutputPrice = OutputPrice;
                product.IdUnit = SelectedUnit.Id;
                product.IdSuplier = SelectedSuplier.Id;

                DataProvider.Instance.Data.SaveChanges();

                // Cập nhật lại QuickPickProducts theo trạng thái mã vạch mới
                var quickItem = QuickPickProducts.FirstOrDefault(x => x.Id == product.Id);
                if (!string.IsNullOrWhiteSpace(product.BarCode))
                {
                    if (quickItem != null) QuickPickProducts.Remove(quickItem);
                }
                else
                {
                    if (quickItem == null) QuickPickProducts.Add(product);
                }
            }
        }

        private void DeleteProductOrdered(object p)
        {
            CartList.Clear();
            CustomerPayment = 0;
        }

        //private void PasteAndScan()
        //{
        //    if (Clipboard.ContainsText())
        //    {
        //        string text = Clipboard.GetText()?.Trim();
        //        if (!string.IsNullOrEmpty(text))
        //        {
        //            ScannedBarCode = text; // Gán mã vào ô
        //            OnScanBarCode();       // Gọi hàm quét luôn
        //        }
        //    }
        //}

        //private void CopyBarCode()
        //{
        //    if (SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.BarCode))
        //    {
        //        Clipboard.SetText(SelectedItem.BarCode);
        //    }
        //}

        private void DeleteObject(object p)
        {
            if (SelectedItem == null) return;

            // Lưu lại ID của sản phẩm trước khi làm sạch giao diện
            string selectedId = SelectedItem.Id;

            var product = DataProvider.Instance.Data.Objects.FirstOrDefault(x => x.Id == selectedId);
            if (product != null)
            {
                // 1. Đánh dấu ẩn sản phẩm trong DB
                product.IsDeleted = true;
                DataProvider.Instance.Data.SaveChanges();

                // 2. Gỡ khỏi danh sách chính
                List?.Remove(SelectedItem);

                // 3. Gỡ khỏi danh sách chọn nhanh (Kiểm tra null an toàn)
                if (QuickPickProducts != null)
                {
                    var quickItem = QuickPickProducts.FirstOrDefault(x => x != null && x.Id == selectedId);
                    if (quickItem != null)
                    {
                        QuickPickProducts.Remove(quickItem);
                    }
                }

                // 4. Reset các ô nhập liệu
                ClearInputFiles();
            }
        }

        private void Add(object p)
        {
            if (SelectedUnit == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị đo!");
                return;
            }

            if (SelectedSuplier == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp!");
                return;
            }
            var objectList = new Model.Object() { Id = Guid.NewGuid().ToString(), DisplayName = DisplayName, IdSuplier = SelectedSuplier.Id, IdUnit = SelectedUnit.Id, InputPrice = InputPrice, OutputPrice = OutputPrice, PercentPrice = (int)PercentPrice, BarCode = BarCode, IsDeleted=false };
            DataProvider.Instance.Data.Add(objectList);
            DataProvider.Instance.Data.SaveChanges();
            List.Add(objectList);
            // 4. KIỂM TRA VÀ THÊM NGAY SANG Ô "MÓN BÁN LẺ (KHÔNG MÃ VẠCH)"
            if (string.IsNullOrWhiteSpace(objectList.BarCode))
            {
                // Thêm trực tiếp vào danh sách phím chọn nhanh
                QuickPickProducts.Add(objectList);
            }
            ClearInputFiles();
        }
        private void ClearInputFiles()
        {
            DisplayName = string.Empty;
            SelectedUnit = null;
            SelectedSuplier = null;
            InputPrice = decimal.Zero;
            OutputPrice = decimal.Zero;
            PercentPrice = decimal.Zero;
            BarCode = string.Empty;
        }
        private void PrintInvoice()
        {
            if (CartList == null || !CartList.Any()) return;

            try
            {
                // =========================================================================
                // BƯỚC 1: LƯU DOANH SỐ VÀO DATABASE (BẮT BỘC ĐỂ NHẢY BÁO CÁO)
                // =========================================================================

                // 1. Tạo hóa đơn Output
                var newOutput = new Model.Output()
                {
                    DateOutput = DateTime.Now
                };
                DataProvider.Instance.Data.Outputs.Add(newOutput);

                // Lưu trước để SQL Server cấp Id (IDENTITY) cho newOutput
                DataProvider.Instance.Data.SaveChanges();

                // 2. Lưu từng chi tiết sản phẩm bán vào OutputInfo để tính Doanh Thu
                foreach (var item in CartList)
                {
                    var newOutputInfo = new Model.OutputInfo()
                    {
                        // KHÔNG gán Id vì SQL Server tự tăng (INT IDENTITY)
                        IdOutput = newOutput.Id,
                        IdObject = item.Product.Id,
                        IdCustomer = SelectedCustomer != null ? SelectedCustomer.Id : 1,
                        Count = item.Quantity,
                        Status = "Đã thanh toán"
                    };

                    DataProvider.Instance.Data.OutputInfos.Add(newOutputInfo);
                }

                // Lưu danh sách bán hàng xuống SQL Server
                DataProvider.Instance.Data.SaveChanges();

                // =========================================================================
                // BƯỚC 2: TIẾN HÀNH IN HÓA ĐƠN RA MÁY IN
                // =========================================================================
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument doc = new FlowDocument
                    {
                        PageWidth = 280,
                        ColumnWidth = 280,
                        PagePadding = new Thickness(14, 10, 10, 10),
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 11
                    };

                    // Header: Tên cửa hàng
                    Paragraph storeName = new Paragraph(new Run("VĂN PHÒNG PHẨM MINH ĐỨC"))
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 2)
                    };
                    doc.Blocks.Add(storeName);

                    // Địa chỉ
                    Paragraph address = new Paragraph(new Run("78/49/15 Nguyễn Văn Khối, P. Thông Tây Hội, Gò Vấp"))
                    {
                        FontSize = 10,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    doc.Blocks.Add(address);
                    doc.Blocks.Add(new Paragraph(new Run("----------------------------------")) { TextAlignment = TextAlignment.Center });
                    // Tiêu đề
                    Paragraph title = new Paragraph(new Run("HÓA ĐƠN BÁN HÀNG"))
                    {
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    doc.Blocks.Add(title);
                    doc.Blocks.Add(new Paragraph(new Run("----------------------------------")) { TextAlignment = TextAlignment.Center });
                    var customerName=$"Khách hàng: {(SelectedCustomer != null ? SelectedCustomer.DisplayName : "Khách lẻ")}";
                    doc.Blocks.Add(new Paragraph(new Run(customerName)) {FontWeight=FontWeights.Bold, FontSize = 14, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 4) });
                    // Ngày giờ
                    Paragraph info = new Paragraph(new Run($"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}\n----------------------------------"))
                    {
                        FontSize = 10,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    doc.Blocks.Add(info);
                    // Bảng sản phẩm
                    Table table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 5) };
                    table.Columns.Add(new TableColumn { Width = new GridLength(105) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(28) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(60) });
                    table.Columns.Add(new TableColumn { Width = new GridLength(63) });

                    TableRowGroup rowGroup = new TableRowGroup();

                    TableRow headerRow = new TableRow { FontWeight = FontWeights.Bold };
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Tên SP")) { TextAlignment = TextAlignment.Left }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("SL")) { TextAlignment = TextAlignment.Center }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Đ.Giá")) { TextAlignment = TextAlignment.Right }));
                    headerRow.Cells.Add(new TableCell(new Paragraph(new Run("T.Tiền")) { TextAlignment = TextAlignment.Right }));
                    rowGroup.Rows.Add(headerRow);
                    // 2. Thêm đường gạch phân cách đứt nét ngay bên dưới Header
                    TableRow headerSeparatorRow = new TableRow();
                    TableCell headerSepCell = new TableCell(
                        new Paragraph(new Run("=================================="))
                        {
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 1, 0, 3)
                        }
                    )
                    {
                        ColumnSpan = 4, // Gộp cả 4 cột để gạch kéo dài hết chiều rộng bảng
                        Foreground = System.Windows.Media.Brushes.Black
                    };

                    headerSeparatorRow.Cells.Add(headerSepCell);
                    rowGroup.Rows.Add(headerSeparatorRow);
                    foreach (var item in CartList)
                    {
                        TableRow row = new TableRow();
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product.DisplayName)) { TextAlignment = TextAlignment.Left }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run((item.Product.OutputPrice ?? 0m).ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.TotalPrice.ToString("N0"))) { TextAlignment = TextAlignment.Right }));
                        rowGroup.Rows.Add(row);
                        // THÊM: Dòng gạch phân cách giữa các mặt hàng
                        rowGroup.Rows.Add(CreateSeparatorRow());
                    }

                    table.RowGroups.Add(rowGroup);
                    doc.Blocks.Add(table);

                    // Footer
                    Paragraph footer = new Paragraph();
                    footer.Inlines.Add(new Run($"Số lượng mặt hàng: {CartList.Count}\n") { FontWeight=FontWeights.Bold, FontSize=14});
                    footer.Inlines.Add(new Run("----------------------------------\n"));
                    footer.Inlines.Add(new Run($"TỔNG TIỀN: {GrandTotal:N0} VNĐ\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
                    footer.Inlines.Add(new Run("----------------------------------\n"));
                    footer.Inlines.Add(new Run($"Tiền khách đưa: {CustomerPayment:N0} VNĐ\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
                    footer.Inlines.Add(new Run("----------------------------------\n"));
                    footer.Inlines.Add(new Run($"Tiền thối lại: {ChangeMoney:N0} VNĐ\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
                    footer.Inlines.Add(new Run("----------------------------------\n"));
                    footer.TextAlignment = TextAlignment.Right;
                    footer.Margin = new Thickness(0, 5, 0, 0);

                    doc.Blocks.Add(footer);

                    Paragraph thankYou = new Paragraph(new Run("Cảm ơn quý khách và hẹn gặp lại!"))
                    {
                        TextAlignment = TextAlignment.Center,
                        FontSize = 10,
                        Margin = new Thickness(5, 5, 5, 5)
                    };
                    doc.Blocks.Add(thankYou);
                    doc.Blocks.Add(new Paragraph(new Run("Thank you for all customers and see you again!")) { TextAlignment = TextAlignment.Center, FontSize = 10, Margin = new Thickness(5, 0, 5, 5) });
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    printDialog.PrintDocument(paginator, "In Hóa Đơn Bán Hàng");

                    // =========================================================================
                    // BƯỚC 3: DỌN GIỎ HÀNG
                    // =========================================================================
                    CartList.Clear();
                    CustomerPayment = 0;
                    MessageBox.Show("Thanh toán thành công! Dữ liệu doanh số đã được ghi nhận.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thanh toán & in hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PrintInvoiceViaWifi(string printerIpAddress, int port = 9100)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient(printerIpAddress, port))
                using (var stream = client.GetStream())
                {
                    // 1. Reset máy in ESC/POS (1B 40)
                    stream.Write(new byte[] { 0x1B, 0x40 }, 0, 2);

                    // 2. Lệnh thiết lập LỀ TRÁI = 16 dots (~2mm) để không bị hụt chữ (GS L 16 0)
                    stream.Write(new byte[] { 0x1D, 0x4C, 0x10, 0x00 }, 0, 4);

                    // 3. Lệnh CĂN GIỮA (ESC a 1)
                    byte[] alignCenter = new byte[] { 0x1B, 0x61, 0x01 };
                    // Lệnh CĂN TRÁI (ESC a 0)
                    byte[] alignLeft = new byte[] { 0x1B, 0x61, 0x00 };

                    StringBuilder sb = new StringBuilder();

                    // Căn giữa cho Tiêu đề
                    stream.Write(alignCenter, 0, alignCenter.Length);

                    string headerStr = RemoveVietnameseSign("VAN PHONG PHAM MINH DUC\n") +
                                       RemoveVietnameseSign("78/49/15 NGUYEN VAN KHOI, GO VAP\n") +
                                       "--------------------------------\n" +
                                       "HOA DON BAN HANG\n" +
                                       $"Ngay: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                                       "--------------------------------\n";

                    byte[] headerBytes = Encoding.ASCII.GetBytes(headerStr);
                    stream.Write(headerBytes, 0, headerBytes.Length);

                    // Chuyển về Căn trái để in sản phẩm
                    stream.Write(alignLeft, 0, alignLeft.Length);

                    StringBuilder bodySb = new StringBuilder();
                    foreach (var item in CartList)
                    {
                        string name = RemoveVietnameseSign(item.Product.DisplayName);
                        bodySb.AppendLine(name);
                        // Định dạng dòng số lượng & giá tiền
                        bodySb.AppendLine($"  {item.Quantity} x {item.Product.OutputPrice:N0} = {item.TotalPrice:N0}");
                    }

                    bodySb.AppendLine("--------------------------------");
                    bodySb.AppendLine($"TONG TIEN: {GrandTotal:N0} VND\n");
                    bodySb.AppendLine("--------------------------------\n");
                    bodySb.AppendLine($"Tien khach dua: {CustomerPayment:N0} VND\n");
                    bodySb.AppendLine($"Tien thoi lai:  {ChangeMoney:N0} VND");
                    bodySb.AppendLine("--------------------------------\n");
                    bodySb.AppendLine("Cam on quy khach va hen gap lai!\n");

                    byte[] bodyBytes = Encoding.ASCII.GetBytes(bodySb.ToString());
                    stream.Write(bodyBytes, 0, bodyBytes.Length);

                    // 4. Lệnh cắt giấy tự động (GS V 0)
                    byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x00 };
                    stream.Write(cutPaper, 0, cutPaper.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối máy in Wi-Fi: {ex.Message}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm phụ trợ: Loạị bỏ dấu Tiếng Việt để gửi lệnh ASCII không bị lỗi ký tự mã hóa trên máy in ESC/POS
        private string RemoveVietnameseSign(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            string[] vietnameseSigns = new string[]
            {
        "aàảãáạăằẳẵắặâầẩẫấậ", "AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ",
        "dđ", "DĐ",
        "eèẻẽéẹêềểễếệ", "EÈẺẼÉẸÊỀỂỄẾỆ",
        "iìỉĩíị", "IÌỈĨÍỊ",
        "oòỏõóọôồổỗốộơờởỡớợ", "OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ",
        "uùủũúụưừửữứự", "UÙỦŨÚỤƯỪỬỮỨỰ",
        "yỳỷỹýỵ", "YỲỶỸÝỴ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i += 2)
            {
                for (int j = 0; j < vietnameseSigns[i - 1].Length; j++)
                    str = str.Replace(vietnameseSigns[i - 1][j], vietnameseSigns[i][0]);
            }
            return str;
        }
        // --- HÀM TẠO DÒNG PHÂN CÁCH ---
        TableRow CreateSeparatorRow()
        {
            TableRow sepRow = new TableRow();

            // Gộp tất cả 4 cột lại làm 1 dòng phân cách dài
            TableCell sepCell = new TableCell(
                new Paragraph(new Run("----------------------------------------------------------"))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 2)
                }
            )
            {
                ColumnSpan = 4,
                Foreground = System.Windows.Media.Brushes.Black // Làm mờ đường gạch cho đẹp mắt
            };

            sepRow.Cells.Add(sepCell);
            return sepRow;
        }
        // Xử lý khi quét mã vạch
        //private void OnScanBarCode()
        //{
        //    string code = ScannedBarCode?.Trim();
        //    if (string.IsNullOrEmpty(code)) return;

        //    var product = List.FirstOrDefault(x => string.Equals(x.BarCode?.Trim(), code, StringComparison.OrdinalIgnoreCase));

        //    if (product != null)
        //    {
        //        Task.Run(() => Console.Beep(2500, 100));
        //        LastScannedProduct = product;

        //        // Đổ thông tin lên form phía trên
        //        DisplayName = product.DisplayName;
        //        SelectedUnit = product.IdUnitNavigation;
        //        SelectedSuplier = product.IdSuplierNavigation;
        //        BarCode = product.BarCode;
        //        InputPrice = (decimal)product.InputPrice;
        //        PercentPrice = (decimal)product.PercentPrice;

        //        // Thêm vào giỏ hàng
        //        var existItem = CartList.FirstOrDefault(x => string.Equals(x.Product?.BarCode?.Trim(), code, StringComparison.OrdinalIgnoreCase));
        //        if (existItem != null)
        //        {
        //            existItem.Quantity++;
        //        }
        //        else
        //        {
        //            CartList.Add(new CartItem
        //            {
        //                Product = product,
        //                Quantity = 1
        //            });
        //        }

        //        OnPropertyChanged(nameof(GrandTotal));
        //        OnPropertyChanged(nameof(ChangeMoney));
        //    }
        //    else
        //    {
        //        Task.Run(() => Console.Beep(500, 300));
        //        BarCode = code; // Sản phẩm mới -> Đẩy mã lên ô BarCode nhập mới phía trên
        //    }

        //    // Xóa rỗng để sẵn sàng cho lần quét tiếp theo
        //    ScannedBarCode = string.Empty;
        //}
        private void OnScanBarCode()
        {
            string code = ScannedBarCode?.Trim();
            if (string.IsNullOrEmpty(code)) return;

            // Tìm theo BarCode HOẶC Tên sản phẩm (để hỗ trợ món không mã vạch)
            var product = List.FirstOrDefault(x =>
                string.Equals(x.BarCode?.Trim(), code, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.DisplayName?.Trim(), code, StringComparison.OrdinalIgnoreCase));

            if (product != null)
            {
                Task.Run(() => Console.Beep(2500, 100));

                // Kiểm tra trong giỏ hàng theo Id sản phẩm (chuẩn xác hơn so với chỉ so BarCode)
                var existItem = CartList.FirstOrDefault(x => x.Product?.Id == product.Id);
                if (existItem != null)
                {
                    existItem.Quantity++;
                }
                else
                {
                    CartList.Add(new CartItem
                    {
                        Product = product,
                        Quantity = 1
                    });
                }

                OnPropertyChanged(nameof(GrandTotal));
                OnPropertyChanged(nameof(ChangeMoney));

                DisplayName = product.DisplayName;
                SelectedUnit = product.IdUnitNavigation;
                SelectedSuplier = product.IdSuplierNavigation;
                BarCode = product.BarCode;
                InputPrice = (decimal)product.InputPrice;
                PercentPrice = (decimal)product.PercentPrice;

                // Lưu ý: Chỉ SaveChanges nếu thực sự có chỉnh sửa trong CSDL, tránh gọi thừa khi bán hàng
                // DataProvider.Instance.Data.SaveChanges();
            }
            else
            {
                Task.Run(() => Console.Beep(500, 300));
                BarCode = code;
            }

            ScannedBarCode = string.Empty;
        }
        private void ExportExcel(object p)
        {
            string filePath = "";
            // tạo SaveFileDialog để lưu file excel
            SaveFileDialog dialog = new SaveFileDialog();

            // chỉ lọc ra các file có định dạng Excel
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";

            // Nếu mở file và chọn nơi lưu file thành công sẽ lưu đường dẫn lại dùng
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            // nếu đường dẫn null hoặc rỗng thì báo không hợp lệ và return hàm
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn báo cáo không hợp lệ");
                return;
            }

            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Vo Tan Duc");
                using (ExcelPackage x = new ExcelPackage())
                {
                    // đặt tên người tạo file
                    x.Workbook.Properties.Author = "Đức";

                    // đặt tiêu đề cho file
                    x.Workbook.Properties.Title = "Add data";

                    //Tạo một sheet để làm việc trên đó
                    x.Workbook.Worksheets.Add("Add data sheet");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = x.Workbook.Worksheets[0];

                    // đặt tên cho sheet
                    ws.Name = "Add data sheet";
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";

                    // Tạo danh sách các column header
                    string[] arrColumnHeader = {
                                        "Tên sản phẩm",
                                        "Đơn vị đo",
                                        "Nhà cung cấp",
                                        "Mã vạch",
                                        "Giá nhập",
                                        "%",
                                        "Giá xuất"
        };

                    // lấy ra số lượng cột cần dùng dựa vào số lượng header
                    var countColHeader = arrColumnHeader.Count();

                    // merge các column lại từ column 1 đến số column header
                    // gán giá trị cho cell vừa merge là Thống kê thông tni User Kteam
                    ws.Cells[1, 1].Value = "Danh sách object";
                    ws.Cells[1, 1, 1, countColHeader].Merge = true;
                    // in đậm
                    ws.Cells[1, 1, 1, countColHeader].Style.Font.Bold = true;
                    // căn giữa
                    ws.Cells[1, 1, 1, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 2;

                    //tạo các header từ column header đã tạo từ bên trên
                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        ////set màu thành gray
                        //var fill = cell.Style.Fill;
                        //fill.PatternType = ExcelFillStyle.Solid;
                        //fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                        //căn chỉnh các border
                        var border = cell.Style.Border;
                        border.Bottom.Style =
                            border.Top.Style = ExcelBorderStyle.Hair;
                            border.Left.Style = ExcelBorderStyle.Dotted;
                            border.Right.Style = ExcelBorderStyle.Thin;

                        //gán giá trị
                        cell.Value = item;

                        colIndex++;
                    }

                    // với mỗi item trong danh sách sẽ ghi trên 1 dòng
                    foreach (var item in List)
                    {
                        // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0
                        colIndex = 1;

                        // rowIndex tương ứng từng dòng dữ liệu
                        rowIndex++;

                        //gán giá trị cho từng cell                      
                        ws.Cells[rowIndex, colIndex++].Value = item.DisplayName;
                        // lưu ý phải .ToShortDateString để dữ liệu khi in ra Excel là ngày như ta vẫn thấy.Nếu không sẽ ra tổng số :v
                        ws.Cells[rowIndex, colIndex++].Value = item.IdUnitNavigation.DisplayName;
                        ws.Cells[rowIndex, colIndex++].Value = item.IdSuplierNavigation.DisplayName;
                        ws.Cells[rowIndex, colIndex++].Value = item.BarCode;
                        ws.Cells[rowIndex, colIndex++].Value = item.InputPrice;
                        ws.Cells[rowIndex, colIndex++].Value = item.PercentPrice;
                        ws.Cells[rowIndex, colIndex++].Value = item.OutputPrice;
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    }

                    //Lưu file lại
                    Byte[] bin = x.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception EE)
            {
                MessageBox.Show("Có lỗi khi lưu file!" +EE.ToString());
            }
        }

        private void CalculateOutputPrice()
        {
            OutputPrice = InputPrice * (1 + PercentPrice / 100m);
        }
        #region Thêm cho tính năng Quét mã vạch & Tính tổng tiền

        private string _scannedBarCode;
        public string ScannedBarCode
        {
            get => _scannedBarCode;
            set { _scannedBarCode = value; OnPropertyChanged(); }
        }

        // Danh sách mặt hàng trong giỏ quét hiện tại
        private ObservableCollection<CartItem> _cartList;
        public ObservableCollection<CartItem> CartList
        {
            get => _cartList;
            set { _cartList = value; OnPropertyChanged(); }
        }

        // TỔNG TIỀN GIÁ BÁN CỦA CẢ HÓA ĐƠN
        public decimal GrandTotal => CartList?.Sum(x => x.TotalPrice) ?? 0;
        // 1. TIỀN KHÁCH ĐƯA
        private decimal _customerPayment;
        public decimal CustomerPayment
        {
            get => _customerPayment;
            set
            {
                _customerPayment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeMoney)); // Tự động tính lại tiền thối khi tiền khách đưa thay đổi
            }
        }

        // 2. TIỀN THỐI LẠI (Tự động tính = Tiền khách đưa - Tổng tiền)
        public decimal ChangeMoney => CustomerPayment >= GrandTotal ? CustomerPayment - GrandTotal : 0;
        public ICommand ScanBarCodeCommand { get; set; }

        #endregion
    }
}
