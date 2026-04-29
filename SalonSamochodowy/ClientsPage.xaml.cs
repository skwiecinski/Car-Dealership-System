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

            ClientsList = new ObservableCollection<ClientModel>
            {
                new ClientModel { FullName = "Jan Kowalski", TaxId = "-", PhoneNumber = "500-100-200", Email = "j.kowalski@gmail.com", IsCompany = false },
                new ClientModel { FullName = "Auto-Trans Sp. z o.o.", TaxId = "525-000-11-22", PhoneNumber = "22 450 10 10", Email = "biuro@autotrans.pl", IsCompany = true },
                new ClientModel { FullName = "Marek Nowak", TaxId = "-", PhoneNumber = "601-202-303", Email = "nowak@wp.pl", IsCompany = false }
            };

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

        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is ClientModel selected)
            {
                MessageBox.Show($"Tworzenie nowego zamówienia dla: {selected.FullName}");
            }
        }
    }

    public class ClientModel
    {
        public string FullName { get; set; }
        public string TaxId { get; set; }  
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsCompany { get; set; }
    }
}