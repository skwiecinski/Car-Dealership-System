using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

            WeakReferenceMessenger.Default.Register(this, (SalesRecordsViewModel r, SalonSamochodowy.Messages.LanguageChangedMessage m) =>
            {
                r.BuildFilterLists();
                r.ApplyFilters();
                
                // Refresh Records localization
                var temp = r.Records.ToList();
                r.Records.Clear();
                foreach (var rec in temp) r.Records.Add(rec);
            });
        }

        public ObservableCollection<SalesRecordRow> Records { get; } = new();

        public ObservableCollection<FilterItem> StatusFilters { get; } = new();
        public ObservableCollection<FilterItem> DealershipFilters { get; } = new();
        public ObservableCollection<FilterItem> AdvisorFilters { get; } = new();

        [ObservableProperty] private FilterItem? selectedStatus;
        [ObservableProperty] private FilterItem? selectedDealership;
        [ObservableProperty] private FilterItem? selectedAdvisor;
        [ObservableProperty] private DateTime? dateFrom;
        [ObservableProperty] private DateTime? dateTo;

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string summaryText = "";
        [ObservableProperty] private Visibility emptyVisibility = Visibility.Collapsed;

        public event Action<string>? LoadFailed;

        private string AllStatuses => SalonSamochodowy.Services.LocalizationHelper.GetString("Filter_AllStatuses");
        private string AllDealerships => SalonSamochodowy.Services.LocalizationHelper.GetString("Filter_AllDealerships");
        private string AllAdvisors => SalonSamochodowy.Services.LocalizationHelper.GetString("Filter_AllAdvisors");

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
                string msg = SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_LoadSalesRecordsError");
                LoadFailed?.Invoke(string.Format(msg, ex.Message, ex.InnerException?.Message));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BuildFilterLists()
        {
            var oldStatus = SelectedStatus?.Value;
            var oldDealership = SelectedDealership?.Value;
            var oldAdvisor = SelectedAdvisor?.Value;

            StatusFilters.Clear();
            StatusFilters.Add(new FilterItem(null, AllStatuses));
            foreach (var s in _allRecords.Select(r => r.Status).Distinct().OrderBy(s => s))
                StatusFilters.Add(new FilterItem(s, SalonSamochodowy.Services.LocalizationHelper.GetString(GetStatusKey(s))));

            DealershipFilters.Clear();
            DealershipFilters.Add(new FilterItem(null, AllDealerships));
            foreach (var d in _allRecords.Select(r => r.DealershipName).Distinct().OrderBy(d => d))
                DealershipFilters.Add(new FilterItem(d, d));

            AdvisorFilters.Clear();
            AdvisorFilters.Add(new FilterItem(null, AllAdvisors));
            foreach (var a in _allRecords.Select(r => r.AdvisorName).Distinct().OrderBy(a => a))
                AdvisorFilters.Add(new FilterItem(a, a));

            // Domyslne wartosci i przywracanie wyboru
            SelectedStatus = StatusFilters.FirstOrDefault(x => x.Value == oldStatus) ?? StatusFilters.First();
            SelectedDealership = DealershipFilters.FirstOrDefault(x => x.Value == oldDealership) ?? DealershipFilters.First();
            SelectedAdvisor = AdvisorFilters.FirstOrDefault(x => x.Value == oldAdvisor) ?? AdvisorFilters.First();
        }

        partial void OnSelectedStatusChanged(FilterItem? value) => ApplyFilters();
        partial void OnSelectedDealershipChanged(FilterItem? value) => ApplyFilters();
        partial void OnSelectedAdvisorChanged(FilterItem? value) => ApplyFilters();
        partial void OnDateFromChanged(DateTime? value) => ApplyFilters();
        partial void OnDateToChanged(DateTime? value) => ApplyFilters();

        private void ApplyFilters()
        {
            IEnumerable<SalesRecordRow> filtered = _allRecords;

            if (SelectedStatus?.Value != null)
                filtered = filtered.Where(r => r.Status == SelectedStatus.Value);

            if (SelectedDealership?.Value != null)
                filtered = filtered.Where(r => r.DealershipName == SelectedDealership.Value);

            if (SelectedAdvisor?.Value != null)
                filtered = filtered.Where(r => r.AdvisorName == SelectedAdvisor.Value);

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
            var culture = new System.Globalization.CultureInfo(SalonSamochodowy.Services.LocalizationHelper.GetString("Culture_CurrencyCode"));
            string summaryFmt = SalonSamochodowy.Services.LocalizationHelper.GetString("SalesRecords_Summary");
            SummaryText = string.Format(summaryFmt, list.Count, totalValue.ToString("C0", culture));
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedStatus = StatusFilters.FirstOrDefault();
            SelectedDealership = DealershipFilters.FirstOrDefault();
            SelectedAdvisor = AdvisorFilters.FirstOrDefault();
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
                : SalonSamochodowy.Services.LocalizationHelper.GetString("SalesRecords_UnknownVehicle");

            var clientUser = o.Client?.User;
            var clientName = clientUser != null
                ? $"{clientUser.FirstName} {clientUser.LastName}".Trim()
                : SalonSamochodowy.Services.LocalizationHelper.GetString("SalesRecords_UnknownClient");

            var workerUser = o.Worker?.User;
            var advisorName = workerUser != null
                ? $"{workerUser.FirstName} {workerUser.LastName}".Trim()
                : "—";

            var culture = new System.Globalization.CultureInfo(SalonSamochodowy.Services.LocalizationHelper.GetString("Culture_CurrencyCode"));

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

        public static string GetStatusKey(string dbStatus) => dbStatus switch
        {
            OrderStatuses.Pending => "Status_Pending",
            OrderStatuses.InProgress => "Status_InProgress",
            OrderStatuses.Finished => "Status_Finished",
            OrderStatuses.FinishedAlt => "Status_FinishedAlt",
            OrderStatuses.Reserved => "Status_Reserved",
            OrderStatuses.Canceled => "Status_Canceled",
            "Nowe" => "Status_New",
            _ => $"Status_{dbStatus.Replace(" ", "")}"
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
        public string StatusDisplay => string.IsNullOrEmpty(Status) ? "" : SalonSamochodowy.Services.LocalizationHelper.GetString(SalesRecordsViewModel.GetStatusKey(Status));
    }

    public class FilterItem
    {
        public string? Value { get; }
        public string DisplayValue { get; }
        
        public FilterItem(string? value, string displayValue)
        {
            Value = value;
            DisplayValue = displayValue;
        }

        public override string ToString() => DisplayValue;
    }
}
