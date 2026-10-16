using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SalesManagement_WPF_Core.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        // Khi True (Doanh thu tăng) -> Trả về màu Xanh Lục
        // Khi False (Doanh thu giảm) -> Trả về màu Đỏ
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUp && isUp)
            {
                return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Màu Xanh Green (#2E7D32)
            }
            return new SolidColorBrush(Color.FromRgb(198, 40, 40));     // Màu Đỏ Red (#C62828)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
