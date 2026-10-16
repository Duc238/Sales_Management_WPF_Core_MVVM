using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static OfficeOpenXml.ExcelErrorValue;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            _canExecute = canExecute;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            try
            {
                // Nếu parameter null => dùng default(T)
                if (parameter == null)
                {
                    return _canExecute(default);
                }

                return _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            if (parameter is T t)
                _execute(t);
            else
                _execute(default);
        }
        // Đảm bảo event CanExecuteChanged kết nối với CommandManager như bên dưới:
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
