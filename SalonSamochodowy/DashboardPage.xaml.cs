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
        public ISeries[] VehicleStructureSeries { get; set; } = Array.Empty<ISeries>();

        public ObservableCollection<RecentOrderItem> RecentOrders { get; set; } = new();

        // Definicja stylu tekstu legendy
        public SolidColorPaint LegendTextStyle { get; set; } =
            new SolidColorPaint(new SKColor(138, 141, 152)); // #8A8D98

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

                // --- KPI ---
                var orders = (await uow.SalesOrders.GetAllAsync()).ToList();
                var vehicles = (await uow.Vehicles.GetAllAsync()).ToList();
                var jobs = (await uow.Jobs.GetAllAsync()).ToList();

                var thisMonth = DateTime.Today.AddDays(-DateTime.Today.Day + 1);
                TxtKpiOrders.Text   = orders.Count(o => o.OrderDate >= thisMonth).ToString();
                TxtKpiVehicles.Text = vehicles.Count(v => v.Status == "Dostępny").ToString();
                TxtKpiJobs.Text     = jobs.Count(j => j.Status == "Oczekujące" || j.Status == "W trakcie").ToString();

                // --- Wykres slupkowy: sprzedaz w ostatnich 5 miesiacach ---
                BuildSalesChart(orders);

                // --- Wykres kolowy: struktura pojazdow na placu (po marce) ---
                await BuildVehicleStructureAsync(uow, vehicles);

                // --- Tabela: ostatnie zamowienia ---
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

        private void BuildSalesChart(System.Collections.Generic.IList<SalesOrder> orders)
        {
            var accentColor = new SKColor(91, 89, 232);
            var axisTextColor = new SKColor(138, 141, 152);
            var separatorColor = new SKColor(45, 48, 56);

            // Ostatnie 5 miesiecy (rosnaco)
            var months = Enumerable.Range(0, 5)
                .Select(i => DateTime.Today.AddMonths(-4 + i))
                .Select(d => new { Year = d.Year, Month = d.Month })
                .ToList();

            var values = months.Select(m =>
                orders.Count(o => o.OrderDate.Year == m.Year && o.OrderDate.Month == m.Month)
            ).ToArray();

            var labels = months.Select(m =>
                new DateTime(m.Year, m.Month, 1).ToString("MMM yy")
            ).ToArray();

            SalesSeries = new ISeries[] {
                new ColumnSeries<int> {
                    Values = values,
                    Fill = new SolidColorPaint(accentColor),
                    Name = "Sprzedaż",
                    MaxBarWidth = 35,
                    Rx = 6, Ry = 6
                }
            };
            XAxes = new[] { new Axis { Labels = labels, LabelsPaint = new SolidColorPaint(axisTextColor) } };
            YAxes = new[] {
                new Axis {
                    LabelsPaint = new SolidColorPaint(axisTextColor),
                    SeparatorsPaint = new SolidColorPaint(separatorColor) { StrokeThickness = 1 }
                }
            };

            // Wymuszenie rerender wykresu po nadpisaniu serii
            DataContext = null;
            DataContext = this;
        }

        private async Task BuildVehicleStructureAsync(UnitOfWork uow, System.Collections.Generic.IList<Vehicle> vehicles)
        {
            var trims = (await uow.TrimLevels.GetAllAsync()).ToList();
            var models = (await uow.VehicleModels.GetAllAsync()).ToList();

            // Grupowanie po marce
            int bmwCount = 0, miniCount = 0, otherCount = 0;
            foreach (var v in vehicles)
            {
                var trim = trims.FirstOrDefault(t => t.TrimID == v.TrimID);
                var model = trim != null ? models.FirstOrDefault(m => m.ModelID == trim.ModelID) : null;
                var brand = model?.Brand;
                if (brand == "BMW") bmwCount++;
                else if (brand == "Mini") miniCount++;
                else otherCount++;
            }

            var series = new System.Collections.Generic.List<ISeries>();
            if (bmwCount > 0)
                series.Add(new PieSeries<int> { Values = new[] { bmwCount }, Name = "BMW", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(45, 127, 249)) });
            if (miniCount > 0)
                series.Add(new PieSeries<int> { Values = new[] { miniCount }, Name = "Mini", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(249, 115, 22)) });
            if (otherCount > 0)
                series.Add(new PieSeries<int> { Values = new[] { otherCount }, Name = "Inne", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(138, 141, 152)) });

            VehicleStructureSeries = series.ToArray();
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
