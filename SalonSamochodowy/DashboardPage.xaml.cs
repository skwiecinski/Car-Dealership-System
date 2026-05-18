using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SkiaSharp;

namespace SalonSamochodowy
{
    public partial class DashboardPage : Page
    {
        public ISeries[] SalesSeries { get; set; } = Array.Empty<ISeries>();
        public Axis[] XAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] YAxes { get; set; } = Array.Empty<Axis>();

        public ObservableCollection<RecentOrderItem> RecentOrders { get; set; } = new();

        public DashboardPage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += async (s, e) => await LoadFromDbAsync();
        }

        private async Task LoadFromDbAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var orders = (await uow.SalesOrders.GetAllAsync()).ToList();
                var vehicles = (await uow.Vehicles.GetAllAsync()).ToList();
                var jobs = (await uow.Jobs.GetAllAsync()).ToList();

                var thisMonth = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                TxtKpiOrders.Text   = orders.Count(o => o.OrderDate >= thisMonth).ToString();
                TxtKpiVehicles.Text = vehicles.Count(v => v.Status == "Dostępny").ToString();
                TxtKpiJobs.Text     = jobs.Count(j => j.Status == "Oczekujące" || j.Status == "W trakcie").ToString();

                await BuildEmployeeRankingAsync(uow, orders);

                await BuildRecentOrdersAsync(uow, orders);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować danych dashboardu:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Dashboard",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task BuildEmployeeRankingAsync(UnitOfWork uow, System.Collections.Generic.IList<SalesOrder> orders)
        {
            var accentColor = new SKColor(91, 89, 232);
            var axisTextColor = new SKColor(138, 141, 152);
            var separatorColor = new SKColor(45, 48, 56);

            var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            var thisMonthOrders = orders.Where(o => o.OrderDate >= firstDay && o.OrderDate < nextMonth).ToList();
            var grouped = thisMonthOrders
                .GroupBy(o => o.WorkerID)
                .Select(g => new { WorkerID = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var labels = new System.Collections.Generic.List<string>();
            var values = new System.Collections.Generic.List<int>();
            foreach (var g in grouped)
            {
                var worker = await uow.Workers.GetByIdAsync(g.WorkerID);
                if (worker == null) continue;
                var user = await uow.AppUsers.GetByIdAsync(worker.UserID);
                if (user == null) continue;
                labels.Add($"{user.FirstName} {user.LastName.FirstOrDefault()}.".Trim());
                values.Add(g.Count);
            }

            if (values.Count == 0)
            {
                labels.Add("—");
                values.Add(0);
            }

            SalesSeries = new ISeries[] {
                new ColumnSeries<int> {
                    Values = values.ToArray(),
                    Fill = new SolidColorPaint(accentColor),
                    Name = "Sprzedaż",
                    MaxBarWidth = 35,
                    Rx = 6, Ry = 6
                }
            };
            XAxes = new[] { new Axis { Labels = labels.ToArray(), LabelsPaint = new SolidColorPaint(axisTextColor) } };
            YAxes = new[] {
                new Axis {
                    LabelsPaint = new SolidColorPaint(axisTextColor),
                    SeparatorsPaint = new SolidColorPaint(separatorColor) { StrokeThickness = 1 },
                    MinLimit = 0
                }
            };

            var polishCulture = new System.Globalization.CultureInfo("pl-PL");
            var monthName = firstDay.ToString("MMMM yyyy", polishCulture);
            TxtSalesChartTitle.Text = $"Sprzedaż w {monthName} (Top 5 Pracowników)";

            DataContext = null;
            DataContext = this;
        }

        private async Task BuildRecentOrdersAsync(UnitOfWork uow, System.Collections.Generic.IList<SalesOrder> orders)
        {
            RecentOrders.Clear();

            var trims = (await uow.TrimLevels.GetAllAsync()).ToList();
            var models = (await uow.VehicleModels.GetAllAsync()).ToList();
            var vehicles = (await uow.Vehicles.GetAllAsync()).ToList();
            var clients = (await uow.Clients.GetAllAsync()).ToList();

            var latest = orders.OrderByDescending(o => o.OrderDate).Take(5);
            foreach (var o in latest)
            {
                var vehicle = vehicles.FirstOrDefault(v => v.VehicleID == o.VehicleID);
                var trim = vehicle != null ? trims.FirstOrDefault(t => t.TrimID == vehicle.TrimID) : null;
                var model = trim != null ? models.FirstOrDefault(m => m.ModelID == trim.ModelID) : null;

                string clientName = "—";
                var client = clients.FirstOrDefault(c => c.ClientID == o.ClientID);
                if (client != null)
                {
                    var user = await uow.AppUsers.GetByIdAsync(client.UserID);
                    if (user != null)
                    {
                        clientName = string.IsNullOrWhiteSpace(user.LastName)
                            ? user.FirstName
                            : $"{user.FirstName} {user.LastName}";
                    }
                }

                var (bg, bd, fg) = StatusColors(o.Status);

                RecentOrders.Add(new RecentOrderItem
                {
                    OrderCode        = $"ZAM/{o.OrderDate:yyyy}/{o.OrderID:D4}",
                    ClientName       = clientName,
                    Vehicle          = model != null && trim != null ? $"{model.Brand} {model.ModelName} {trim.TrimName}" : "—",
                    Status           = o.Status,
                    Date             = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                    StatusBackground = bg,
                    StatusBorder     = bd,
                    StatusForeground = fg
                });
            }

            TxtEmptyOrders.Visibility = RecentOrders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static (string bg, string bd, string fg) StatusColors(string status) => status switch
        {
            "Zrealizowane" or "Sfinalizowane" => ("#112C1E", "#2D9A4A", "#44C767"),
            "W realizacji" or "W trakcie"     => ("#332A12", "#D3A125", "#F0B82B"),
            "Anulowane"                       => ("#3D1D1D", "#D34545", "#ED6262"),
            _                                 => ("#1F2536", "#3B82F6", "#60A5FA"),
        };
    }

    public class RecentOrderItem
    {
        public string OrderCode        { get; set; } = "";
        public string ClientName       { get; set; } = "";
        public string Vehicle          { get; set; } = "";
        public string Status           { get; set; } = "";
        public string Date             { get; set; } = "";
        public string StatusBackground { get; set; } = "";
        public string StatusBorder     { get; set; } = "";
        public string StatusForeground { get; set; } = "";
    }
}
