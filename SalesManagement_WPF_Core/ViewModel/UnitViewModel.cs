using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SalesManagement_WPF_Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class UnitViewModel : BaseViewModel
    {
        private ObservableCollection<Unit> _List;
        public ObservableCollection<Unit> List { get => _List; set { _List = value; OnPropertyChanged(); } }
        private Unit _SelectedItem;
        public Unit SelectedItem
        {
            get => _SelectedItem;
            set
            {
                _SelectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    DisplayName = SelectedItem.DisplayName;
                }
            }
        }

        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; OnPropertyChanged(); } }
        public UnitViewModel()
        {
            List = new ObservableCollection<Unit>(DataProvider.Instance.Data.Units);
            AddCommand = new RelayCommand<object>((p) => 
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return false;

                var displayList = DataProvider.Instance.Data.Units.Where(x => x.DisplayName == DisplayName);
                if (displayList == null || displayList.Count() != 0)
                    return false;

                return true;
            }, (p) => AddUnit(p));
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => Delete(p));
            ExportExcelCommand=new RelayCommand<object>((p)=>true,(p) => ExportExcel(p));
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
                DisplayName = string.Empty;
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
                                        "Tên đơn vị đo"
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

        private void AddUnit(object p)
        {
            var unit = new Unit() { DisplayName = DisplayName };

            DataProvider.Instance.Data.Units.Add(unit);
            DataProvider.Instance.Data.SaveChanges();

            List.Add(unit);
            DisplayName = string.Empty;
        }

        public ICommand AddCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
    }
}
