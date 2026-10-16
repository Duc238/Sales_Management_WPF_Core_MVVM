using SalesManagement_WPF_Core.Model;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SalesManagement_WPF_Core
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var db = new SalesManagementContext())
                {
                    // Tự động tạo DB 'SalesManagement' và các Table trên máy khách nếu chưa có
                    db.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối CSDL: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
