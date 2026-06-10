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
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class SalesPanelViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderService _orderService;

        public SalesPanelViewModel(IUnitOfWork uow, IOrderService orderService)
        {
            _uow = uow;
            _orderService = orderService;

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register(this, (SalesPanelViewModel r, SalonSamochodowy.Messages.DataChangedMessage m) =>
            {
                r.IsLoaded = false;
            });
        }

        public ObservableCollection<SalesOrderModel> ReservedSales { get; } = new();
        public ObservableCollection<SalesOrderModel> ServicingSales { get; } = new();
        public ObservableCollection<SalesOrderModel> ReadySales { get; } = new();

        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private Visibility reservedEmptyVisibility = Visibility.Visible;
        [ObservableProperty] private Visibility servicingEmptyVisibility = Visibility.Visible;
        [ObservableProperty] private Visibility readyEmptyVisibility = Visibility.Visible;

        public event Action<string>? LoadFailed;
        public event Action<string>? OperationCompleted;

        public bool IsLoaded { get; set; } = false;

        public async Task LoadDataAsync()
        {
            if (IsLoaded) return;
            IsLoading = true;
            try
            {
                
                var orders = await _uow.SalesOrders.FindWithIncludesAsync(
                    o => o.Status != OrderStatuses.Finished &&
                         o.Status != OrderStatuses.FinishedAlt &&
                         o.Status != OrderStatuses.Canceled,
                    o => o.Vehicle.Trim.Model,
                    o => o.Vehicle.Engine,
                    o => o.Client.User,
                    o => o.Worker.User,
                    o => o.Vehicle.Jobs
                );

                
                
                var allJobs = await _uow.Jobs.GetAllWithIncludesAsync(j => j.Feature);
                var jobsLookup = allJobs.ToLookup(j => j.VehicleID);

                ReservedSales.Clear();
                ServicingSales.Clear();
                ReadySales.Clear();

                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                {
                    var vehicle = order.Vehicle;
                    var client = order.Client;
                    
                    var modelName = vehicle?.Trim?.Model != null 
                        ? $"{vehicle.Trim.Model.Brand} {vehicle.Trim.Model.ModelName}" 
                        : SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_UnknownModel");
                    
                    var trimName = vehicle?.Trim?.TrimName ?? "—";
                    var engineInfo = vehicle?.Engine != null 
                        ? $"{vehicle.Engine.EngineSize} {vehicle.Engine.EngineName} ({vehicle.Engine.Power} KM)" 
                        : "—";

                    var vehicleJobs = jobsLookup[order.VehicleID].ToList();
                    
                    bool hasJobs = vehicleJobs.Any();
                    bool allJobsFinished = hasJobs && vehicleJobs.All(j => j.Status == JobStatuses.Finished || j.Status == "FinishedJob");
                    bool hasActiveJobs = hasJobs && vehicleJobs.Any(j => j.Status == JobStatuses.Pending || j.Status == JobStatuses.InProgress || j.Status == "PendingJob" || j.Status == "InProgressJob");

                    string jobsStatusText = SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_NoJobs");
                    string jobsDescription = "";
                    if (hasJobs)
                    {
                        var finishedCount = vehicleJobs.Count(j => j.Status == JobStatuses.Finished || j.Status == "FinishedJob");
                        jobsStatusText = string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_JobsCompleted"), finishedCount, vehicleJobs.Count);
                        jobsDescription = string.Join("\n", vehicleJobs.Select(j => $"• {j.Feature.FeatureName} ({j.Status})"));
                    }

                    var model = new SalesOrderModel
                    {
                        OrderID = order.OrderID,
                        ClientName = (client != null && client.User != null) ? $"{client.User.FirstName} {client.User.LastName}" : SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_UnknownClient"),
                        ClientContact = (client != null && client.User != null) ? $"{SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_Tel")} {client.Phone} | {SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_Email")} {client.User.Email}" : (client != null ? $"{SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_Tel")} {client.Phone}" : "—"),
                        VehicleName = $"{modelName} {trimName}",
                        VehicleVin = vehicle?.VIN ?? "—",
                        EngineInfo = engineInfo,
                        OrderDateText = order.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                        FinalPriceText = order.FinalPrice.ToString("C2", new System.Globalization.CultureInfo("pl-PL")),
                        JobsStatus = jobsStatusText,
                        JobsDescription = jobsDescription,
                        HasJobs = hasJobs,
                        OrderStatus = order.Status
                    };

                    
                    
                    
                    
                    if (!hasJobs)
                    {
                        ReservedSales.Add(model);
                    }
                    else if (hasActiveJobs)
                    {
                        ServicingSales.Add(model);
                    }
                    else if (allJobsFinished)
                    {
                        ReadySales.Add(model);
                    }
                }

                ReservedEmptyVisibility = ReservedSales.Any() ? Visibility.Collapsed : Visibility.Visible;
                ServicingEmptyVisibility = ServicingSales.Any() ? Visibility.Collapsed : Visibility.Visible;
                ReadyEmptyVisibility = ReadySales.Any() ? Visibility.Collapsed : Visibility.Visible;

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_LoadError"), ex.Message));
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task RefreshDataAsync()
        {
            IsLoaded = false;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task CloseSuccessAsync(SalesOrderModel? order)
        {
            if (order == null) return;

            var result = MessageBox.Show(
                string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_ConfirmSuccess"), order.VehicleName, order.VehicleVin, order.FinalPriceText),
                SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_ConfirmSuccessTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                await _orderService.UpdateOrderStatusAsync(order.OrderID, OrderStatuses.Finished);
                OperationCompleted?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_MsgSuccess"));
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_FinalizeError"), ex.Message), SalonSamochodowy.Services.LocalizationHelper.GetString("Global_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CloseFailAsync(SalesOrderModel? order)
        {
            if (order == null) return;

            var result = MessageBox.Show(
                string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_ConfirmFail"), order.VehicleName, order.VehicleVin),
                SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_ConfirmFailTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                await _orderService.UpdateOrderStatusAsync(order.OrderID, OrderStatuses.Canceled);
                OperationCompleted?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_MsgFail"));
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_CancelError"), ex.Message), SalonSamochodowy.Services.LocalizationHelper.GetString("Global_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class SalesOrderModel
    {
        public int OrderID { get; set; }
        public string ClientName { get; set; } = "";
        public string ClientContact { get; set; } = "";
        public string VehicleName { get; set; } = "";
        public string VehicleVin { get; set; } = "";
        public string EngineInfo { get; set; } = "";
        public string OrderDateText { get; set; } = "";
        public string FinalPriceText { get; set; } = "";
        public string JobsStatus { get; set; } = "";
        public string JobsDescription { get; set; } = "";
        public bool HasJobs { get; set; }
        public string OrderStatus { get; set; } = "";
    }
}
