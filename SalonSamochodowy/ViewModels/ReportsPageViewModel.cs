using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public class ClientItem
    {
        public int ClientID { get; set; }
        public string Display { get; set; } = "";
    }

    public partial class ReportsPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;
        private readonly ReportGeneratorService _reportService;

        [ObservableProperty] private ObservableCollection<ClientItem> availableClients = new();
        [ObservableProperty] private ClientItem? selectedClient;

        public ReportsPageViewModel(IUnitOfWork uow, ReportGeneratorService reportService)
        {
            _uow = uow;
            _reportService = reportService;
        }

        public async Task LoadDataAsync()
        {
            AvailableClients.Clear();
            var clients = await _uow.Clients.GetAllWithIncludesAsync(c => c.User);
            foreach (var c in clients.OrderBy(x => x.User?.LastName).ThenBy(x => x.User?.FirstName))
            {
                AvailableClients.Add(new ClientItem
                {
                    ClientID = c.ClientID,
                    Display = $"{c.User?.FirstName} {c.User?.LastName} ({c.User?.Email})"
                });
            }
        }

        [RelayCommand]
        private async Task GenerateOrdersReportAsync()
        {
            try
            {
                var orders = await _uow.SalesOrders.GetAllWithIncludesAsync(o => o.Vehicle.Trim.Model, o => o.Client.User);
                var filePath = await _reportService.GenerateOrdersByStatusReportAsync(orders);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateClientReportAsync()
        {
            if (SelectedClient == null)
            {
                MessageBox.Show("Proszę wybrać klienta z listy.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var client = (await _uow.Clients.FindWithIncludesAsync(c => c.ClientID == SelectedClient.ClientID, c => c.User)).FirstOrDefault();
                var orders = (await _uow.SalesOrders.FindWithIncludesAsync(o => o.ClientID == SelectedClient.ClientID, o => o.Vehicle.Trim.Model, o => o.Client.User)).ToList();

                var filePath = await _reportService.GenerateClientReportAsync(client!, orders);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateRankingsReportAsync()
        {
            try
            {
                var orders = await _uow.SalesOrders.GetAllAsync();
                var workers = await _uow.Workers.GetAllAsync();
                var users = await _uow.AppUsers.GetAllAsync();
                var dealerships = await _uow.Dealerships.GetAllAsync();

                var filePath = await _reportService.GenerateSalesRankingsReportAsync(orders, workers, users, dealerships);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateFullDumpReportAsync()
        {
            try
            {
                var dealerships = await _uow.Dealerships.GetAllAsync();
                var workers = await _uow.Workers.GetAllAsync();
                var users = await _uow.AppUsers.GetAllAsync();
                var orders = await _uow.SalesOrders.GetAllAsync();
                var clients = await _uow.Clients.GetAllWithIncludesAsync(c => c.User);
                var vehicles = await _uow.Vehicles.GetAllWithIncludesAsync(v => v.Trim.Model);
                var engines = await _uow.Engines.GetAllAsync();
                var trims = await _uow.TrimLevels.GetAllWithIncludesAsync(t => t.Model);
                var features = await _uow.Features.GetAllAsync();
                var models = await _uow.VehicleModels.GetAllAsync();
                var jobs = await _uow.Jobs.GetAllWithIncludesAsync(j => j.Feature, j => j.Worker.User);

                var filePath = await _reportService.GenerateFullDatabaseDumpReportAsync(dealerships, workers, users, orders, clients, vehicles, engines, trims, features, models, jobs);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateFinancialReportAsync()
        {
            try
            {
                var orders = await _uow.SalesOrders.GetAllAsync(); // only need basic info (dates, prices, status)
                var filePath = await _reportService.GenerateMonthlyRevenueReportAsync(orders);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateTrendsReportAsync()
        {
            try
            {
                var orders = await _uow.SalesOrders.GetAllWithIncludesAsync(o => o.Vehicle.Trim.Model);
                var filePath = await _reportService.GenerateModelPopularityReportAsync(orders);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task GenerateServiceReportAsync()
        {
            try
            {
                var jobs = await _uow.Jobs.GetAllWithIncludesAsync(j => j.Worker.User, j => j.Feature);
                var filePath = await _reportService.GenerateServiceEfficiencyReportAsync(jobs);
                OpenFile(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
    }
}
