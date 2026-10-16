using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class CartItem:BaseViewModel
    {
        private Model.Object _product;
        public Model.Object Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice)); // Cập nhật Thành tiền dòng này
            }
        }

        // Thành tiền giá bán = Số lượng * Giá bán (OutputPrice)
        public decimal TotalPrice => (Product?.OutputPrice ?? 0) * Quantity;
    }
}
