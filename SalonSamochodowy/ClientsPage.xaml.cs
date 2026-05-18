using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SalonSamochodowy
{
    public partial class ClientsPage : Page
    {
        public ObservableCollection<ClientModel> ClientsList { get; set; }

        public ClientsPage()
        {
            InitializeComponent();

            // Pusta kolekcja - backend wypelni z bazy
            ClientsList = new ObservableCollection<ClientModel>();

            this.DataContext = this;
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

        private void NewClient_Click(object sender, RoutedEventArgs e)
        {
            // Otwarcie dialogu nowego klienta
            var dialog = new AddClientWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.Result is ClientModel newClient)
            {
                // Tymczasowo dodajemy do lokalnej listy.
                // TODO (backend): zapis do bazy przez UnitOfWork, potem odswiezenie listy z bazy.
                ClientsList.Add(newClient);
            }
        }

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is not ClientModel selected)
                return;

            // Przejscie do formularza zamowienia z wybranym klientem.
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

            // TODO (backend): pelna edycja danych w bazie.
            // Na razie otwieramy ten sam dialog z prewypelnionymi polami i pozwalamy zaktualizowac.
            MessageBox.Show(
                $"Edycja klienta '{selected.FullName}' - do implementacji.",
                "Edytuj dane",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    public class ClientModel
    {
        public string FullName { get; set; } = "";
        public string TaxId { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsCompany { get; set; }
    }
}
