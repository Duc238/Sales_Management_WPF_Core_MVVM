using SalesManagement_WPF_Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SalesManagement_WPF_Core.DucUserControl
{
    /// <summary>
    /// Interaction logic for ucListObject.xaml
    /// </summary>
    public partial class ucListObject : UserControl
    {
        private string _barcodeBuffer = ""; // Biến tạm tích lũy dãy số máy quét gõ vào
        public ucListObject()
        {
            InitializeComponent();
            this.Loaded += (s, e) => FocusBarCodeBox();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra xem người dùng có đang gõ vào các ô TextBox nhập sản phẩm phía trên không
            var focusedElement = Keyboard.FocusedElement as FrameworkElement;

            // Nếu người dùng không gõ vào ô nhập tên, giá,... thì MỌI THAO TÁC/MÁY QUÉT đều tự động đưa về ô txtBarCode
            if (focusedElement != null && focusedElement is TextBox tb && tb != txtBarCode)
            {
                return; // Để người dùng nhập dữ liệu form trên bình thường
            }

            // Nếu con trỏ chưa nằm ở txtBarCode thì ép con trỏ quay lại txtBarCode
            if (!txtBarCode.IsFocused)
            {
                FocusBarCodeBox();
            }
        }
        private void FocusBarCodeBox()
        {
            txtBarCode.Focus();
            Keyboard.Focus(txtBarCode);
        }
        //private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    // 1. Nếu bạn ĐANG CLICK CHUỘT gõ chữ vào các ô như: Tên sản phẩm, Giá nhập, Tiền khách đưa... 
        //    // thì ưu tiên cho gõ bàn phím bình thường, không can thiệp.
        //    if (e.OriginalSource is TextBox activeTextBox && activeTextBox.Name != "txtBarCode")
        //    {
        //        return;
        //    }

        //    // 2. Khi máy quét kết thúc lượt quét bằng phím ENTER
        //    if (e.Key == Key.Enter)
        //    {
        //        if (!string.IsNullOrEmpty(_barcodeBuffer))
        //        {
        //            if (DataContext is ObjectViewModel vm)
        //            {
        //                // Đẩy dãy số vừa quét vào ViewModel và kích hoạt lệnh bán hàng
        //                vm.ScannedBarCode = _barcodeBuffer;
        //                if (vm.ScanBarCodeCommand != null && vm.ScanBarCodeCommand.CanExecute(null))
        //                {
        //                    vm.ScanBarCodeCommand.Execute(null);
        //                }
        //            }

        //            _barcodeBuffer = ""; // Xóa bộ nhớ đệm cho lần quét tiếp theo
        //        }
        //        e.Handled = true; // Đã xử lý xong phím Enter
        //    }
        //    else
        //    {
        //        // 3. Tích lũy từng ký tự mà máy quét phát ra
        //        char keyChar = KeyToChar(e.Key);
        //        if (keyChar != '\0')
        //        {
        //            _barcodeBuffer += keyChar;
        //        }
        //    }
        //}

        // Hàm phụ trợ chuyển đổi từ Key WPF sang ký tự Chữ/Số
        //private char KeyToChar(Key key)
        //{
        //    if (key >= Key.D0 && key <= Key.D9) return (char)('0' + (key - Key.D0));
        //    if (key >= Key.NumPad0 && key <= Key.NumPad9) return (char)('0' + (key - Key.NumPad0));
        //    if (key >= Key.A && key <= Key.Z) return (char)('A' + (key - Key.A));
        //    return '\0';
        //}
    }
}