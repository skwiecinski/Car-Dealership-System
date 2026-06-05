using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class ClientsPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        private readonly IClientService _clientService;
        public ClientsPageViewModel(IUnitOfWork uow, IClientService clientService)
        {
            _uow = uow;
            _clientService = clientService;
        }
        public ObservableCollection<ClientModel> ClientsList { get; } = new();

        [ObservableProperty] private ClientModel? selectedClient;
        [ObservableProperty] private Visibility detailsVisibility = Visibility.Collapsed;
        [ObservableProperty] private string selectedClientName = "";
        [ObservableProperty] private string selectedClientType = "";

        public event Action? NewClientRequested;
        public event Action<ClientModel>? EditClientRequested;
        public event Action<ClientModel>? DeleteClientRequested;
        public event Action<ClientModel>? NewOrderRequested;
        public event Action<string>? LoadFailed;

        partial void OnSelectedClientChanged(ClientModel? value)
        {
            if (value == null)
            {
                DetailsVisibility = Visibility.Collapsed;
                return;
            }
            DetailsVisibility = Visibility.Visible;
            SelectedClientName = value.FullName;
            SelectedClientType = value.IsCompany ? "Klient Biznesowy" : "Klient Indywidualny";
        }

        public async Task LoadFromDbAsync()
        {
            try
            {
                var uow = _uow;

                var clients = await _clientService.GetAllClientsAsync();
                ClientsList.Clear();
                foreach (var c in clients)
                {
                    ClientsList.Add(new ClientModel
                    {
                        ClientId    = c.ClientID,
                        UserId      = c.UserID,
                        FullName    = c.FullName,
                        TaxId       = string.IsNullOrWhiteSpace(c.NIP) ? "-" : c.NIP,
                        PhoneNumber = c.Phone,
                        Email       = c.Email,
                        IsCompany   = c.IsCompany
                    });
                }
            }
            catch (Exception ex)
            {
                LoadFailed?.Invoke($"Nie udało się załadować klientów z bazy:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private void NewClient() => NewClientRequested?.Invoke();

        [RelayCommand]
        private void NewOrder()
        {
            if (SelectedClient != null)
                NewOrderRequested?.Invoke(SelectedClient);
        }

        [RelayCommand]
        private void EditClient()
        {
            if (SelectedClient != null)
                EditClientRequested?.Invoke(SelectedClient);
        }

        [RelayCommand]
        private void DeleteClient()
        {
            if (SelectedClient != null)
                DeleteClientRequested?.Invoke(SelectedClient);
        }
    }

    public class ClientModel
    {
        public int ClientId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string TaxId { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsCompany { get; set; }
    }
}