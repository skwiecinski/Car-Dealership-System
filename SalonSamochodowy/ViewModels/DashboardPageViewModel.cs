using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;
using SkiaSharp;

namespace SalonSamochodowy.ViewModels
{
    public partial class DashboardPageViewModel : ObservableObject
    {
        private readonly ICatalogService _catalogService;
    private readonly IClientService _clientService;
    private readonly IVehicleService _vehicleService;
    private readonly IOrderService _orderService;
    private readonly IJobService _jobService;

        public DashboardPageViewModel(ICatalogService catalogService, IClientService clientService, IVehicleService vehicleService, IOrderService orderService, IJobService jobService)
        {
            _catalogService = catalogService;
            _clientService = clientService;
        _vehicleService = vehicleService;
        _orderService = orderService;
        _jobService = jobService;

            WeakReferenceMessenger.Default.Register(this, (DashboardPageViewModel r, SalonSamochodowy.Messages.LanguageChangedMessage m) =>
            {
                // Refresh title
                var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
                var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var monthName = firstDay.ToString("MMMM yyyy", currentCulture);
                var titleFormat = LocalizationHelper.GetString("Dash_SalesChartTitle");
                r.SalesChartTitle = string.Format(titleFormat, monthName);

                // Refresh orders
                foreach (var order in r.RecentOrders)
                {
                    order.RefreshLocalization();
                }
            });
        }
        [ObservableProperty] private string kpiOrders = "—";
        [ObservableProperty] private string kpiVehicles = "—";
        [ObservableProperty] private string kpiJobs = "—";

        [ObservableProperty] private string salesChartTitle = "Sprzedaż w bieżącym miesiącu (Top 5 Pracowników)";

        [ObservableProperty] private ISeries[] salesSeries = Array.Empty<ISeries>();
        [ObservableProperty] private Axis[] xAxes = Array.Empty<Axis>();
        [ObservableProperty] private Axis[] yAxes = Array.Empty<Axis>();

        [ObservableProperty] private Visibility emptyOrdersVisibility = Visibility.Collapsed;

        public ObservableCollection<RecentOrderItem> RecentOrders { get; } = new();

        public event Action<string>? LoadFailed;

        public async Task LoadAsync()
        {
            try
            {
                

                var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                KpiOrders   = (await _orderService.GetActiveOrdersCountAsync()).ToString();
                KpiVehicles = (await _vehicleService.GetAvailableVehiclesCountAsync()).ToString();
                KpiJobs     = (await _jobService.GetActiveJobsCountAsync()).ToString();

                var thisMonthOrders = (await _orderService.GetOrdersSinceAsync(firstDayOfMonth)).ToList();

                await BuildEmployeeRankingAsync(thisMonthOrders, firstDayOfMonth);
                await BuildRecentOrdersAsync();
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować danych dashboardu:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private async Task BuildEmployeeRankingAsync(IList<SalesOrder> thisMonthOrders, DateTime firstDay)
        {
            var accentColor = new SKColor(91, 89, 232);
            var axisTextColor = new SKColor(138, 141, 152);
            var separatorColor = new SKColor(45, 48, 56);

            var validOrders = thisMonthOrders
                .Where(o => o.Status != OrderStatuses.Canceled)
                .ToList();

            var grouped = validOrders
                .GroupBy(o => o.WorkerID)
                .Select(g => new { WorkerID = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var labels = new List<string>();
            var values = new List<int>();
            foreach (var g in grouped)
            {
                var workerDto = await _catalogService.GetWorkerByIdAsync(g.WorkerID);
                if (workerDto == null) continue;
                labels.Add($"{workerDto.FullName.Split(' ')[0]} {workerDto.FullName.Split(' ').LastOrDefault()?.FirstOrDefault()}.".Trim());
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

            var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            var monthName = firstDay.ToString("MMMM yyyy", currentCulture);
            
            var titleFormat = LocalizationHelper.GetString("Dash_SalesChartTitle");
            SalesChartTitle = string.Format(titleFormat, monthName);
        }

        private async Task BuildRecentOrdersAsync()
        {
            RecentOrders.Clear();

            var latest = await _orderService.GetRecentOrdersAsync(5);

            foreach (var o in latest)
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(o.VehicleID);
                var trim = vehicle != null ? await _catalogService.GetTrimByIdAsync(vehicle.TrimID) : null;
                var model = trim != null ? await _catalogService.GetModelByIdAsync(trim.ModelID) : null;

                string clientName = "—";
                var client = await _clientService.GetClientByIdAsync(o.ClientID);
                if (client != null)
                {
                    clientName = client.FullName;
                }

                var (bg, bd, fg) = StatusColors(o.Status);

                string locStatusKey = o.Status switch
                {
                    OrderStatuses.Pending => "Status_Pending",
                    OrderStatuses.InProgress => "Status_InProgress",
                    OrderStatuses.Finished => "Status_Finished",
                    OrderStatuses.FinishedAlt => "Status_FinishedAlt",
                    OrderStatuses.Reserved => "Status_Reserved",
                    OrderStatuses.Canceled => "Status_Canceled",
                    _ => null
                };
                string locStatus = o.Status;

                RecentOrders.Add(new RecentOrderItem
                {
                    OrderCode        = $"ZAM/{o.OrderDate:yyyy}/{o.OrderID:D4}",
                    ClientName       = clientName,
                    Vehicle          = model != null && trim != null ? $"{model.Brand} {model.ModelName} {trim.TrimName}" : "—",
                    Status           = locStatus,
                    StatusKey        = locStatusKey ?? "",
                    Date             = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                    StatusBackground = bg,
                    StatusBorder     = bd,
                    StatusForeground = fg
                });
            }

            EmptyOrdersVisibility = RecentOrders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static (string bg, string bd, string fg) StatusColors(string status) => status switch
        {
            OrderStatuses.Finished or OrderStatuses.FinishedAlt => ("#112C1E", "#2D9A4A", "#44C767"),
            OrderStatuses.InProgress     => ("#332A12", "#D3A125", "#F0B82B"),
            OrderStatuses.Canceled                       => ("#3D1D1D", "#D34545", "#ED6262"),
            _                                 => ("#1F2536", "#3B82F6", "#60A5FA"),
        };
    }

    public class RecentOrderItem : ObservableObject
    {
        public string OrderCode        { get; set; } = "";
        public string ClientName       { get; set; } = "";
        public string Vehicle          { get; set; } = "";
        public string Status           { get; set; } = "";
        public string StatusKey        { get; set; } = "";
        public string DisplayStatus => !string.IsNullOrEmpty(StatusKey) ? SalonSamochodowy.Services.LocalizationHelper.GetString(StatusKey) : Status;
        public string Date             { get; set; } = "";
        public string StatusBackground { get; set; } = "";
        public string StatusBorder     { get; set; } = "";
        public string StatusForeground { get; set; } = "";

        public void RefreshLocalization()
        {
            OnPropertyChanged(nameof(DisplayStatus));
        }
    }
}
