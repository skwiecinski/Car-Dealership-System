using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class SalesRecordsViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;

        // Pelna lista wszystkich rekordow (cache do filtrowania w pamieci UI po jednym pobraniu z bazy)
        private List<SalesRecordRow> _allRecords = new();

        public SalesRecordsViewModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public ObservableCollection<SalesRecordRow> Records { get; } = new();

        public ObservableCollection<string> StatusFilters { get; } = new();
        public ObservableCollection<string> DealershipFilters { get; } = new();
        public ObservableCollection<string> AdvisorFilters { get; } = new();

        [ObservableProperty] private string? selectedStatus;
        [ObservableProperty] private string? selectedDealership;
        [ObservableProperty] private string? selectedAdvisor;
        [ObservableProperty] private DateTime? dateFrom;
        [ObservableProperty] private DateTime? dateTo;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string summaryText = "";
        [ObservableProperty] private Visibility emptyVisibility = Visibility.Collapsed;

        public event Action<string>? LoadFailed;

        private const string AllStatuses = "Wszystkie statusy";
        private const string AllDealerships = "Wszystkie salony";
        private const string AllAdvisors = "Wszyscy doradcy";

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var orders = await _orderService.GetAllOrdersWithDetailsAsync();

                _allRecords = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(MapToRow)
                    .ToList();

                // Wypelnienie list filtrow z faktycznych danych
                BuildFilterLists();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować ewidencji sprzedaży:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BuildFilterLists()
        {
            StatusFilters.Clear();
            StatusFilters.Add(AllStatuses);
            foreach (var s in _allRecords.Select(r => r.Status).Distinct().OrderBy(s => s))
                StatusFilters.Add(s);

            DealershipFilters.Clear();
            DealershipFilters.Add(AllDealerships);
            foreach (var d in _allRecords.Select(r => r.DealershipName).Distinct().OrderBy(d => d))
                DealershipFilters.Add(d);

            AdvisorFilters.Clear();
            AdvisorFilters.Add(AllAdvisors);
            foreach (var a in _allRecords.Select(r => r.AdvisorName).Distinct().OrderBy(a => a))
                AdvisorFilters.Add(a);

            // Domyslne wartosci
            if (SelectedStatus == null) SelectedStatus = AllStatuses;
            if (SelectedDealership == null) SelectedDealership = AllDealerships;
            if (SelectedAdvisor == null) SelectedAdvisor = AllAdvisors;
        }

        partial void OnSelectedStatusChanged(string? value) => ApplyFilters();
        partial void OnSelectedDealershipChanged(string? value) => ApplyFilters();
        partial void OnSelectedAdvisorChanged(string? value) => ApplyFilters();
        partial void OnDateFromChanged(DateTime? value) => ApplyFilters();
        partial void OnDateToChanged(DateTime? value) => ApplyFilters();

        private void ApplyFilters()
        {
            IEnumerable<SalesRecordRow> filtered = _allRecords;

            if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != AllStatuses)
                filtered = filtered.Where(r => r.Status == SelectedStatus);

            if (!string.IsNullOrEmpty(SelectedDealership) && SelectedDealership != AllDealerships)
                filtered = filtered.Where(r => r.DealershipName == SelectedDealership);

            if (!string.IsNullOrEmpty(SelectedAdvisor) && SelectedAdvisor != AllAdvisors)
                filtered = filtered.Where(r => r.AdvisorName == SelectedAdvisor);

            if (DateFrom.HasValue)
                filtered = filtered.Where(r => r.OrderDate >= DateFrom.Value.Date);

            if (DateTo.HasValue)
                filtered = filtered.Where(r => r.OrderDate <= DateTo.Value.Date.AddDays(1).AddTicks(-1));

            var list = filtered.ToList();

            Records.Clear();
            foreach (var r in list)
                Records.Add(r);

            EmptyVisibility = Records.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            var totalValue = list.Sum(r => r.FinalPrice);
            var culture = new System.Globalization.CultureInfo("pl-PL");
            SummaryText = $"Liczba transakcji: {list.Count}   •   Łączna wartość: {totalValue.ToString("C0", culture)}";
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedStatus = AllStatuses;
            SelectedDealership = AllDealerships;
            SelectedAdvisor = AllAdvisors;
            DateFrom = null;
            DateTo = null;
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadDataAsync();

        private static SalesRecordRow MapToRow(SalesOrder o)
        {
            var model = o.Vehicle?.Trim?.Model;
            var vehicleName = model != null
                ? $"{model.Brand} {model.ModelName} {o.Vehicle?.Trim?.TrimName}".Trim()
                : "Nieznany pojazd";

            var clientUser = o.Client?.User;
            var clientName = clientUser != null
                ? $"{clientUser.FirstName} {clientUser.LastName}".Trim()
                : "Nieznany klient";

            var workerUser = o.Worker?.User;
            var advisorName = workerUser != null
                ? $"{workerUser.FirstName} {workerUser.LastName}".Trim()
                : "—";

            var culture = new System.Globalization.CultureInfo("pl-PL");

            return new SalesRecordRow
            {
                OrderID        = o.OrderID,
                OrderCode      = $"ZAM/{o.OrderDate:yyyy}/{o.OrderID:D4}",
                VehicleName    = vehicleName,
                Vin            = o.Vehicle?.VIN ?? "—",
                ClientName     = clientName,
                ClientEmail    = clientUser?.Email ?? "—",
                AdvisorName    = advisorName,
                DealershipName = o.Dealership?.Name ?? "—",
                OrderDate      = o.OrderDate,
                OrderDateText  = o.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                FinalPrice     = o.FinalPrice,
                FinalPriceText = o.FinalPrice.ToString("C0", culture),
                Status         = o.Status,
                StatusColor    = StatusColor(o.Status)
            };
        }

        private static string StatusColor(string status) => status switch
        {
            OrderStatuses.Finished or OrderStatuses.FinishedAlt => "#44C767",
            OrderStatuses.InProgress or OrderStatuses.Pending   => "#F0B82B",
            OrderStatuses.Reserved                              => "#5B59E8",
            OrderStatuses.Canceled                              => "#ED6262",
            _                                                   => "#8A8D98",
        };
    }

    public class SalesRecordRow
    {
        public int    OrderID        { get; set; }
        public string OrderCode      { get; set; } = "";
        public string VehicleName    { get; set; } = "";
        public string Vin            { get; set; } = "";
        public string ClientName     { get; set; } = "";
        public string ClientEmail    { get; set; } = "";
        public string AdvisorName    { get; set; } = "";
        public string DealershipName { get; set; } = "";
        public DateTime OrderDate     { get; set; }
        public string OrderDateText  { get; set; } = "";
        public decimal FinalPrice    { get; set; }
        public string FinalPriceText { get; set; } = "";
        public string Status         { get; set; } = "";
        public string StatusColor    { get; set; } = "";
    }
}
