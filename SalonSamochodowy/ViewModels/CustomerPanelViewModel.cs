using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class CustomerPanelViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public CustomerPanelViewModel(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [ObservableProperty] private string welcomeText = "Witaj!";
        [ObservableProperty] private string clientDetailsText = "";
        [ObservableProperty] private string ongoingCountText = "Trwające zamówienia (0)";
        [ObservableProperty] private string completedCountText = "Zakończone zamówienia (0)";

        [ObservableProperty] private Visibility emptyOngoingVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility emptyCompletedVisibility = Visibility.Collapsed;

        [ObservableProperty] private Visibility ongoingListVisibility = Visibility.Collapsed;
        [ObservableProperty] private Visibility completedListVisibility = Visibility.Collapsed;

        public ObservableCollection<CustomerOrderItem> OngoingOrders { get; } = new();
        public ObservableCollection<CustomerOrderItem> CompletedOrders { get; } = new();

        public event Action<string>? LoadFailed;

        public async Task LoadAsync()
        {
            try
            {
                var currentUser = SessionContext.CurrentUser;
                if (currentUser == null) return;

                WelcomeText = $"Witaj, {currentUser.FirstName} {currentUser.LastName}!";

                
                var clients = await _uow.Clients.FindWithIncludesAsync(c => c.UserID == currentUser.UserID);
                var client = clients.FirstOrDefault();
                if (client == null)
                {
                    ClientDetailsText = $"E-mail: {currentUser.Email} | Użytkownik nie jest powiązany z żadnym profilem klienta.";
                    OngoingOrders.Clear();
                    CompletedOrders.Clear();
                    OngoingCountText = "Trwające zamówienia (0)";
                    CompletedCountText = "Zakończone zamówienia (0)";
                    EmptyOngoingVisibility = Visibility.Visible;
                    EmptyCompletedVisibility = Visibility.Visible;
                    OngoingListVisibility = Visibility.Collapsed;
                    CompletedListVisibility = Visibility.Collapsed;
                    return;
                }

                ClientDetailsText = $"Telefon: {client.Phone} | Email: {currentUser.Email}" + 
                                    (string.IsNullOrWhiteSpace(client.NIP) ? "" : $" | NIP: {client.NIP}");

                
                var orders = await _uow.SalesOrders.FindWithIncludesAsync(
                    o => o.ClientID == client.ClientID,
                    o => o.Vehicle.Trim.Model,
                    o => o.Vehicle.Engine,
                    o => o.Dealership,
                    o => o.Worker.User
                );

                var ongoingList = new List<CustomerOrderItem>();
                var completedList = new List<CustomerOrderItem>();

                var completedStatuses = new[]
                {
                    OrderStatuses.Finished,
                    OrderStatuses.FinishedAlt,
                    OrderStatuses.Canceled
                };

                foreach (var o in orders.OrderByDescending(o => o.OrderDate))
                {
                    var vehicle = o.Vehicle;
                    var trim = vehicle?.Trim;
                    var model = trim?.Model;
                    var engine = vehicle?.Engine;

                    string vehicleInfo = model != null && trim != null 
                        ? $"{model.Brand} {model.ModelName} {trim.TrimName}" 
                        : "Nieznany pojazd";

                    string engineInfo = engine != null 
                        ? $"{engine.EngineSize} {engine.EngineName} ({engine.Power} KM)" 
                        : "—";

                    string workerInfo = o.Worker != null && o.Worker.User != null
                        ? $"{o.Worker.User.FirstName} {o.Worker.User.LastName}"
                        : "—";

                    var (bg, bd, fg) = StatusColors(o.Status);

                    var item = new CustomerOrderItem
                    {
                        OrderCode = $"ZAM/{o.OrderDate:yyyy}/{o.OrderID:D4}",
                        VehicleInfo = vehicleInfo,
                        EngineInfo = engineInfo,
                        DealershipName = o.Dealership?.Name ?? "—",
                        DealershipCity = o.Dealership?.City ?? "",
                        DealershipAddress = o.Dealership?.Address ?? "",
                        WorkerName = workerInfo,
                        Date = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        Price = $"{o.FinalPrice:N0} PLN",
                        Status = o.Status,
                        StatusBackground = bg,
                        StatusBorder = bd,
                        StatusForeground = fg
                    };

                    if (completedStatuses.Contains(o.Status))
                    {
                        completedList.Add(item);
                    }
                    else
                    {
                        ongoingList.Add(item);
                    }
                }

                OngoingOrders.Clear();
                foreach (var item in ongoingList) OngoingOrders.Add(item);

                CompletedOrders.Clear();
                foreach (var item in completedList) CompletedOrders.Add(item);

                OngoingCountText = $"Trwające zamówienia ({OngoingOrders.Count})";
                CompletedCountText = $"Zakończone zamówienia ({CompletedOrders.Count})";

                EmptyOngoingVisibility = OngoingOrders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                EmptyCompletedVisibility = CompletedOrders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

                OngoingListVisibility = OngoingOrders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                CompletedListVisibility = CompletedOrders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować panelu klienta:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        private static (string bg, string bd, string fg) StatusColors(string status) => status switch
        {
            OrderStatuses.Finished or OrderStatuses.FinishedAlt => ("#112C1E", "#2D9A4A", "#44C767"),
            OrderStatuses.InProgress or "W realizacji" => ("#332A12", "#D3A125", "#F0B82B"),
            OrderStatuses.Canceled => ("#3D1D1D", "#D34545", "#ED6262"),
            _ => ("#1F2536", "#3B82F6", "#60A5FA"),
        };
    }

    public class CustomerOrderItem
    {
        public string OrderCode { get; set; } = "";
        public string VehicleInfo { get; set; } = "";
        public string EngineInfo { get; set; } = "";
        public string DealershipName { get; set; } = "";
        public string DealershipAddress { get; set; } = "";
        public string DealershipCity { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Price { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusBackground { get; set; } = "";
        public string StatusBorder { get; set; } = "";
        public string StatusForeground { get; set; } = "";
    }
}
