using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy
{
    public partial class ClientsPage : Page
    {
        public ObservableCollection<ClientModel> ClientsList { get; set; }

        public ClientsPage()
        {
            InitializeComponent();

            ClientsList = new ObservableCollection<ClientModel>();
            this.DataContext = this;

            // Ladowanie klientow z bazy przy starcie strony
            Loaded += async (s, e) => await LoadClientsFromDbAsync();
        }

        /// <summary>
        /// Pobiera z bazy wszystkich klientow (rola "Klient") i ich dane z AppUser.
        /// </summary>
        private async Task LoadClientsFromDbAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var clients = await uow.Clients.GetAllAsync();

                ClientsList.Clear();

                foreach (var c in clients)
                {
                    // Doczytanie usera klienta (Client.UserID -> AppUser)
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
                System.Windows.MessageBox.Show(
                    $"Nie udało się załadować klientów z bazy:\n{ex.Message}",
                    "Klienci",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is ClientModel selected)
            {
                DetailsBorder.Visibility = Visibility.Visible;
                TxtClientName.Text = selected.FullName;
                TxtClientType.Text = selected.IsCompany ? "Klient Biznesowy" : "Klient Indywidualny";
            }
        }

        private async void NewClient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddClientWindow
            {
                Owner = Window.GetWindow(this)
            };

            // AddClientWindow sam zapisuje do bazy. Po sukcesie odswiezamy liste.
            if (dialog.ShowDialog() == true)
            {
                await LoadClientsFromDbAsync();
            }
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is not ClientModel selected)
                return;

            // TODO (backend): przekazac selected do CreateOrderViewModel.WybranyKlient.
            if (NavigationService != null)
            {
                NavigationService.Navigate(new CreateOrder());
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is not ClientModel selected)
                return;

            // TODO: edycja danych klienta w bazie
            System.Windows.MessageBox.Show(
                $"Edycja klienta '{selected.FullName}' — do implementacji.",
                "Edytuj dane",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
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
