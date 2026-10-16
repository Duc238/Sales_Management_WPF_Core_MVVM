using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SalesManagement_WPF_Core.Behaviors
{
    public enum ResizeDirection
    {
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class ManualResizeBehavior : Behavior<FrameworkElement>
    {
        private bool _isResizing;
        private Point _startMousePosition;
        private double _startWidth;
        private double _startHeight;
        private double _startLeft;
        private double _startTop;
        private Window _targetWindow;

        public ResizeDirection Direction { get; set; } = ResizeDirection.BottomRight;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;
            base.OnDetaching();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _targetWindow = Window.GetWindow(AssociatedObject);
                if (_targetWindow != null)
                {
                    _isResizing = true;
                    // Lấy vị trí chuột tuyệt đối trên màn hình
                    _startMousePosition = AssociatedObject.PointToScreen(e.GetPosition(AssociatedObject));
                    _startWidth = _targetWindow.ActualWidth;
                    _startHeight = _targetWindow.ActualHeight;
                    _startLeft = _targetWindow.Left;
                    _startTop = _targetWindow.Top;

                    AssociatedObject.CaptureMouse();
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing && _targetWindow != null)
            {
                Point currentMouse = AssociatedObject.PointToScreen(e.GetPosition(AssociatedObject));
                double deltaX = currentMouse.X - _startMousePosition.X;
                double deltaY = currentMouse.Y - _startMousePosition.Y;

                double newWidth = _startWidth;
                double newHeight = _startHeight;
                double newLeft = _startLeft;
                double newTop = _startTop;

                // Xử lý chiều ngang (Width và Left)
                if (Direction == ResizeDirection.Left || Direction == ResizeDirection.TopLeft || Direction == ResizeDirection.BottomLeft)
                {
                    // Kéo từ mép trái: Giảm Width, tăng Left
                    newWidth = _startWidth - deltaX;
                    newLeft = _startLeft + deltaX;
                }
                else if (Direction == ResizeDirection.Right || Direction == ResizeDirection.TopRight || Direction == ResizeDirection.BottomRight)
                {
                    // Kéo từ mép phải: Tăng Width
                    newWidth = _startWidth + deltaX;
                }

                // Xử lý chiều dọc (Height và Top)
                if (Direction == ResizeDirection.Top || Direction == ResizeDirection.TopLeft || Direction == ResizeDirection.TopRight)
                {
                    // Kéo từ mép trên: Giảm Height, tăng Top
                    newHeight = _startHeight - deltaY;
                    newTop = _startTop + deltaY;
                }
                else if (Direction == ResizeDirection.Bottom || Direction == ResizeDirection.BottomLeft || Direction == ResizeDirection.BottomRight)
                {
                    // Kéo từ mép dưới: Tăng Height
                    newHeight = _startHeight + deltaY;
                }

                // Áp dụng MinWidth/MinHeight để đảm bảo cửa sổ không bị kéo quá nhỏ
                // và chỉ cập nhật vị trí nếu kích thước hợp lệ
                if (newWidth >= _targetWindow.MinWidth)
                {
                    _targetWindow.Width = newWidth;
                    _targetWindow.Left = newLeft;
                }
                if (newHeight >= _targetWindow.MinHeight)
                {
                    _targetWindow.Height = newHeight;
                    _targetWindow.Top = newTop;
                }
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isResizing)
            {
                _isResizing = false;
                AssociatedObject.ReleaseMouseCapture();
            }
        }
    }
}
