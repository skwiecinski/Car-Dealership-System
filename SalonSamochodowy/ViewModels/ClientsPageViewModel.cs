using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class ClientsPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public ClientsPageViewModel(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public ObservableCollection<ClientModel> ClientsList { get; } = new();

        [ObservableProperty] private ClientModel? selectedClient;
        [ObservableProperty] private Visibility detailsVisibility = Visibility.Collapsed;
        [ObservableProperty] private string selectedClientName = "";
        [ObservableProperty] private string selectedClientType = "";

        public event Action? NewClientRequested;
        public event Action<ClientModel>? EditClientRequested;
        public event Action<ClientModel>? NewOrderRequested;
        public event Action<string>? LoadFailed;

        partial void OnSelectedClientChanged(ClientModel? value)
        {
            if (value == null)
            {
                DetailsVisibility = Visibility.Collapsed;
                return;
            }
            DetailsVisibility    = Visibility.Visible;
            SelectedClientName   = value.FullName;
            SelectedClientType   = value.IsCompany ? "Klient Biznesowy" : "Klient Indywidualny";
        }

        public async Task LoadFromDbAsync()
        {
            try
            {
                var uow = _uow;

                var clients = await uow.Clients.GetAllAsync();

                ClientsList.Clear();
                foreach (var c in clients)
                {
                    var user = await uow.AppUsers.GetByIdAsync(c.UserID);
                    if (user == null) continue;

                    var fullName = string.IsNullOrWhiteSpace(user.LastName)
                        ? user.FirstName
                        : $"{user.FirstName} {user.LastName}";

                    var isCompany = !string.IsNullOrWhiteSpace(c.NIP);

                    ClientsList.Add(new ClientModel
                    {
                        FullName    = fullName,
                        TaxId       = string.IsNullOrWhiteSpace(c.NIP) ? "-" : c.NIP!,
                        PhoneNumber = c.Phone ?? "",
                        Email       = user.Email,
                        IsCompany   = isCompany
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
    }

    public class ClientModel
    {
        public string FullName    { get; set; } = "";
        public string TaxId       { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email       { get; set; } = "";
        public bool   IsCompany   { get; set; }
    }
}
