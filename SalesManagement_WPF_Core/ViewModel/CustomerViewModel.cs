using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SalesManagement_WPF_Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class CustomerViewModel : BaseViewModel, IDataErrorInfo
    {
        private ObservableCollection<Customer> _List;
        public ObservableCollection<Customer> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        private string _Phone;
        public string Phone { get => _Phone; set { _Phone = value; OnPropertyChanged(); } }
        private string _Email;
        public string Email { get => _Email; set { _Email = value; OnPropertyChanged();} }
        private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
        private string _MoreInfo;
        public string MoreInfo { get => _MoreInfo; set { _MoreInfo = value; OnPropertyChanged(); } }
        private string _Address;
        public string Address { get => _Address; set { _Address = value; OnPropertyChanged(); } }
        private DateTime? _ContractDate;
        public DateTime? ContractDate { get => _ContractDate; set { _ContractDate = value; OnPropertyChanged(); } }
        private Customer _SelectedItem;
        public Customer SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                    Phone = SelectedItem.Phone;
                    Email = SelectedItem.Email;
                    Address = SelectedItem.Address;
                    MoreInfo = SelectedItem.MoreInfo;
                    ContractDate = (DateTime)SelectedItem.ContractDate;
                }
            }
        }

        public CustomerViewModel()
        {
            List = new ObservableCollection<Customer>(DataProvider.Instance.Data.Customers);
            AddCommand = new RelayCommand<object>((p) =>
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                var displayList = DataProvider.Instance.Data.Customers.Where(x => x.DisplayName == DisplayName);
                if (displayList == null || displayList.Count() != 0)
                    return false;
                return true;
            }, (p) => Add(p));
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem!=null, (p) => { Delete(p); });
            ExportExcelCommand = new RelayCommand<object>((p) => true, (p) => ExportExcel(p));
        }

        private void Delete(object p)
        {
            if (SelectedItem == null) return;

            // 1. Tìm khách hàng trong DB
            var customer = DataProvider.Instance.Data.Customers.FirstOrDefault(x => x.Id == SelectedItem.Id);
            if (customer != null)
            {
                // 2. Đánh dấu ẩn thay vì dùng DataProvider.Instance.Data.Remove()
                customer.IsDeleted = true;
                DataProvider.Instance.Data.SaveChanges();

                // 3. Gỡ khỏi danh sách hiển thị trên giao diện
                List.Remove(SelectedItem);

                // 4. Reset các ô nhập liệu
                ClearInputFileds();
            }
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
                                        "STT",
                                        "Tên khách hàng",
                                        "Địa chỉ",
                                        "Điện thoại",
                                        "Email",
                                        "Thông tin thêm",
                                        "Ngày hợp tác"
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
                        ws.Cells[rowIndex, colIndex++].Value = item.Id;
                        ws.Cells[rowIndex, colIndex++].Value = item.DisplayName;
                        // lưu ý phải .ToShortDateString để dữ liệu khi in ra Excel là ngày như ta vẫn thấy.Nếu không sẽ ra tổng số :v
                        ws.Cells[rowIndex, colIndex++].Value = item.Address;
                        ws.Cells[rowIndex, colIndex++].Value = item.Phone;
                        ws.Cells[rowIndex, colIndex++].Value = item.Email;
                        ws.Cells[rowIndex, colIndex++].Value = item.MoreInfo;
                        ws.Cells[rowIndex, colIndex++].Value = item.ContractDate.Value.ToShortDateString();
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
                MessageBox.Show("Có lỗi khi lưu file!" + EE.ToString());
            }
        }

        private void Add(object p)
        {
            var customerList = new Customer() { DisplayName = DisplayName, Address = Address, Email = Email, Phone = Phone, MoreInfo = MoreInfo, ContractDate = ContractDate };
            DataProvider.Instance.Data.Customers.Add(customerList);
            DataProvider.Instance.Data.SaveChanges();
            List.Add(customerList);
            ClearInputFileds();
        }
        private void ClearInputFileds()
        {
            DisplayName = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            Email = string.Empty;
            MoreInfo = string.Empty;
            ContractDate = null;
        }
        public ICommand AddCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        // --- Triển khai IDataErrorInfo ---
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == nameof(Email))
                {
                    if (!string.IsNullOrWhiteSpace(Email))
                    {
                        // RegEx kiểm tra định dạng email
                        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                        if (!Regex.IsMatch(Email, pattern))
                        {
                            result = "Email không đúng định dạng";
                        }
                    }
                }

                return result;
            }
        }
    }
}
