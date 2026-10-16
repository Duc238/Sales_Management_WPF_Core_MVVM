// Thêm LiveCharts
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using SalesManagement_WPF_Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesManagement_WPF_Core.ViewModel
{
    public class ReportViewModel : BaseViewModel
    {
        // -----------------------------------------------------
        // 1. DATA CHO COMBOBOX (Giữ nguyên code cũ của bạn)
        // -----------------------------------------------------
        public List<int> DayList { get; } = Enumerable.Range(1, 31).ToList();
        public List<int> MonthList { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> YearList => Enumerable.Range(2020, DateTime.Now.Year - 2020 + 1).Reverse().ToList();

        // -----------------------------------------------------
        // 2. SELECTED VALUES & LOGIC REFRESH
        // -----------------------------------------------------
        private int _selectedDay; public int SelectedDay { get => _selectedDay; set { _selectedDay = value; OnPropertyChanged(); LoadReportData(); } }
        private int _selectedMonth; public int SelectedMonth { get => _selectedMonth; set { _selectedMonth = value; OnPropertyChanged(); LoadReportData(); } }
        private int _selectedYear; public int SelectedYear { get => _selectedYear; set { _selectedYear = value; OnPropertyChanged(); LoadReportData(); } }

        // -----------------------------------------------------
        // 3. PROPERTY HIỂN THỊ DOANH THU & SO SÁNH
        // -----------------------------------------------------
        private double _dailyRevenue; public double DailyRevenue { get => _dailyRevenue; set { _dailyRevenue = value; OnPropertyChanged(); } }
        private double _monthlyRevenue; public double MonthlyRevenue { get => _monthlyRevenue; set { _monthlyRevenue = value; OnPropertyChanged(); } }
        private double _yearlyRevenue; public double YearlyRevenue { get => _yearlyRevenue; set { _yearlyRevenue = value; OnPropertyChanged(); } }

        // Property So sánh Ngày
        public double DailyChangePercent => GetChangePercent(DailyRevenue, PrevDailyRevenue);
        public bool DailyIsUp => DailyRevenue >= PrevDailyRevenue;
        public PackIconKind DailyArrowKind => DailyIsUp ? PackIconKind.ArrowUpThin : PackIconKind.ArrowDownThin;
        private double PrevDailyRevenue { get; set; } // Doanh thu ngày trước đó (để tính toán)

        // Property So sánh Tháng
        public double MonthlyChangePercent => GetChangePercent(MonthlyRevenue, PrevMonthlyRevenue);
        public bool MonthlyIsUp => MonthlyRevenue >= PrevMonthlyRevenue;
        public PackIconKind MonthlyArrowKind => MonthlyIsUp ? PackIconKind.ArrowUpThin : PackIconKind.ArrowDownThin;
        private double PrevMonthlyRevenue { get; set; } // Doanh thu tháng trước đó

        // Property So sánh Năm
        public double YearlyChangePercent => GetChangePercent(YearlyRevenue, PrevYearlyRevenue);
        public bool YearlyIsUp => YearlyRevenue >= PrevYearlyRevenue;
        public PackIconKind YearlyArrowKind => YearlyIsUp ? PackIconKind.ArrowUpThin : PackIconKind.ArrowDownThin;
        private double PrevYearlyRevenue { get; set; } // Doanh thu năm trước đó

        // -----------------------------------------------------
        // 4. PROPERTY CHO BIỂU ĐỒ LIVECHARTS
        // -----------------------------------------------------
        public SeriesCollection ChartSeries { get; set; }
        public string[] XAxisLabels { get; set; }
        // Formatter cho trục Y (định dạng số tiền doanh thu)
        public Func<double, string> YAxisLabelFormatter { get; set; } = value =>
        {
            double absValue = Math.Abs(value);

            if (absValue >= 1_000_000_000)
                return (value / 1_000_000_000).ToString("0.##") + " tỷ VNĐ";

            if (absValue >= 1_000_000)
                return (value / 1_000_000).ToString("0.##") + " triệu VNĐ";

            return value.ToString("N0") + " VNĐ";
        };

        // -----------------------------------------------------
        // CONSTRUCTOR
        // -----------------------------------------------------
        public ReportViewModel()
        {
            // Mặc định chọn Ngày/Tháng/Năm hiện tại
            _selectedDay = DateTime.Now.Day;
            _selectedMonth = DateTime.Now.Month;
            _selectedYear = DateTime.Now.Year;

            // Khởi tạo đối tượng biểu đồ
            ChartSeries = new SeriesCollection();

            LoadReportData();
        }

        // -----------------------------------------------------
        // MAIN LOGIC: TÍNH TOÁN DỮ LIỆU & BIỂU ĐỒ
        // -----------------------------------------------------
        private void LoadReportData()
        {
            if (SelectedYear == 0 || SelectedMonth == 0 || SelectedDay == 0) return;

            try
            {
                // 1. Tải danh sách hóa đơn từ DB ra RAM (Tránh lỗi LINQ to Entities với DateTime)
                // Lưu ý: Đổi OutputInfoes/OutputDate theo đúng tên bảng/cột trong DB của bạn
                // 1. Tải dữ liệu kèm theo các bảng liên quan (nếu dùng EF Core)
                var allOutputs = DataProvider.Instance.Data.OutputInfos
                    .Include(x => x.IdOutputNavigation)
                    .Include(x => x.IdObjectNavigation)
                    .ToList();
                // 2. Tính toán an toàn tuyệt đối chống NullReferenceException
                DailyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation?.DateOutput != null &&
                                x.IdOutputNavigation.DateOutput.Day == SelectedDay &&
                                x.IdOutputNavigation.DateOutput.Month == SelectedMonth &&
                                x.IdOutputNavigation.DateOutput.Year == SelectedYear)
                    .Sum(x => (x.IdObjectNavigation?.OutputPrice ?? 0) * (x.Count ?? 0));

                // Ngày trước đó
                DateTime selectedDateTime = new DateTime(SelectedYear, SelectedMonth, SelectedDay);
                DateTime prevDate = selectedDateTime.AddDays(-1);

                PrevDailyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Date == prevDate.Date)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));


                // 3. TÍNH DOANH THU THÁNG
                MonthlyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Month == SelectedMonth &&
                                x.IdOutputNavigation.DateOutput.Year == SelectedYear)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));

                DateTime prevMonthDate = selectedDateTime.AddMonths(-1);
                PrevMonthlyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Month == prevMonthDate.Month &&
                                x.IdOutputNavigation.DateOutput.Year == prevMonthDate.Year)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));


                // 4. TÍNH DOANH THU NĂM
                YearlyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Year == SelectedYear)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));

                PrevYearlyRevenue = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Year == SelectedYear - 1)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));

                // Thông báo cập nhật UI cho các thuộc tính tính phần trăm
                OnPropertyChanged(nameof(DailyRevenue));
                OnPropertyChanged(nameof(DailyChangePercent));
                OnPropertyChanged(nameof(DailyArrowKind));
                OnPropertyChanged(nameof(DailyIsUp));
                OnPropertyChanged(nameof(MonthlyRevenue));
                OnPropertyChanged(nameof(MonthlyChangePercent));
                OnPropertyChanged(nameof(MonthlyArrowKind));
                OnPropertyChanged(nameof(MonthlyIsUp));
                OnPropertyChanged(nameof(YearlyRevenue));
                OnPropertyChanged(nameof(YearlyChangePercent));
                OnPropertyChanged(nameof(YearlyArrowKind));
                OnPropertyChanged(nameof(YearlyIsUp));

                // Cập nhật lại biểu đồ
                UpdateRevenueChart(allOutputs);
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có
            }
        }

        private void UpdateRevenueChart(List<Model.OutputInfo> allOutputs)
        {
            ChartSeries.Clear();

            int daysInMonth = DateTime.DaysInMonth(SelectedYear, SelectedMonth);
            string[] labels = new string[daysInMonth];
            ChartValues<double> revenueValues = new ChartValues<double>();
            ChartValues<double> offsetValues = new ChartValues<double>();  // Dữ liệu cho Đường (nằm TRÊN cột)
            for (int i = 1; i <= daysInMonth; i++)
            {
                labels[i - 1] = i.ToString();

                double dailySum = (double)allOutputs
                    .Where(x => x.IdOutputNavigation.DateOutput.Day == i &&
                                x.IdOutputNavigation.DateOutput.Month == SelectedMonth &&
                                x.IdOutputNavigation.DateOutput.Year == SelectedYear)
                    .Sum(x => (x.IdObjectNavigation.OutputPrice ?? 0) * (x.Count ?? 0));

                revenueValues.Add(dailySum);
            }

            XAxisLabels = labels;
            OnPropertyChanged(nameof(XAxisLabels));

            ChartSeries.Add(new ColumnSeries
            {
                Title = $"Doanh thu Thg {SelectedMonth}/{SelectedYear}",
                Values = revenueValues,
                Fill = System.Windows.Media.Brushes.DodgerBlue
            });
        }

        // Hàm phụ trợ tính phần trăm thay đổi
        private double GetChangePercent(double current, double previous)
        {
            if (previous == 0) return current == 0 ? 0 : 1; // Trường hợp tháng trước không có doanh thu
            return (current - previous) / previous;
        }
    }
}