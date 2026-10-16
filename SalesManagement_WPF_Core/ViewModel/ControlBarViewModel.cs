using SalesManagement_WPF_Core.DucUserControl;
using SalesManagement_WPF_Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class ControlBarViewModel : BaseViewModel
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public ControlBarViewModel()
        {
            OpenContextMenuCommand = new RelayCommand<ToggleButton>((p) => { return true; }, (p) => { OpenContextMenu(p); });
            CloseContextMenuCommand = new RelayCommand<ToggleButton>((p) => { return true; }, (p) => { p.IsChecked = false; });
            ShowUnitCommand = new RelayCommand<object>((p) => true, (p) => ShowUnit(p));
            ShowSuplierCommand = new RelayCommand<object>((p) => true, (p) => ShowSuplier(p));
            ShowCustomerCommand = new RelayCommand<object>((p) => true, (p) => ShowCustomer(p));
            ShowObjectCommand = new RelayCommand<object>((p) => true, (p) => ShowObject(p));
            CloseWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => CloseWindow(p));
            MaximizeWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => MaximizeWindow(p));
            MinimizeWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => MinimizeWindow(p));
            MouseMoveWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => MouseMoveWindow(p));
            ReportRevenueCommmand=new RelayCommand<object>((p) => true, (p) => CurrentView = new ucRevenueReport());
        }

        private void MouseMoveWindow(UserControl p)
        {
            FrameworkElement window=GetWindowParent(p);
            var w = window as Window;
            if(w!=null)
            {
                w.DragMove();
            }
        }

        private void MinimizeWindow(UserControl p)
        {
            FrameworkElement window = GetWindowParent(p);
            var w = window as Window;
            if (w != null)
            {
                if (w.WindowState != WindowState.Minimized)
                    w.WindowState = WindowState.Minimized;
                else
                    w.WindowState = WindowState.Maximized;
            }
        }

        private void MaximizeWindow(UserControl p)
        {
            FrameworkElement window = GetWindowParent(p);
            var w = window as Window;
            if (w != null)
            {
                if (w.WindowState != WindowState.Maximized)
                    w.WindowState = WindowState.Maximized;
                else
                    w.WindowState = WindowState.Normal;
            }
        }

        private void CloseWindow(UserControl p)
        {
            FrameworkElement window=GetWindowParent(p);
            var w= window as Window;
            if(w!=null)
            {
                w.Close();
            }
        }

        private void ShowObject(object p)
        {
            CurrentView = new ucListObject();
        }

        private void ShowCustomer(object p)
        {
            CurrentView = new ucListCustomer();
        }

        private void ShowSuplier(object p)
        {
            CurrentView = new ucListSuplier();
        }

        private void ShowUnit(object p)
        {
            CurrentView = new ucListUnit();
        }
        public void OpenContextMenu(ToggleButton p)
        {
            if (p.ContextMenu == null)
                return;
            if (p.ContextMenu.IsOpen)
            {
                // Đóng ContextMenu
                p.ContextMenu.IsOpen = false;
                // Đổi icon về Hamburger
                p.IsChecked = false;
            }
            else
            {
                // Mở ContextMenu
                p.ContextMenu.PlacementTarget = p;
                p.ContextMenu.Placement = PlacementMode.Bottom;
                p.ContextMenu.IsOpen = true;
                // Đổi icon thành mũi tên
                p.IsChecked = true;
            }
        }
        FrameworkElement GetWindowParent(UserControl p)
        {
            FrameworkElement parent = p;

            while (parent.Parent != null)
            {
                parent = parent.Parent as FrameworkElement;
            }

            return parent;
        }
        #region Commands
        public ICommand OpenContextMenuCommand { get; set; }
        public ICommand CloseContextMenuCommand { get; set; }
        public ICommand ShowUnitCommand { get; set; }
        public ICommand ShowSuplierCommand { get; set; }
        public ICommand ShowCustomerCommand { get; set; }
        public ICommand ShowObjectCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }
        public ICommand MouseMoveWindowCommand { get; set; }
        public ICommand ReportRevenueCommmand { get; set; }
        #endregion
    }
}
