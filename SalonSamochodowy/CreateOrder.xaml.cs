using System.Windows;
using System.Windows.Controls;

namespace SalonSamochodowy
{
    /// <summary>
    /// Interaction logic for CreateOrder.xaml
    /// </summary>
    public partial class CreateOrder : Page
    {
        public CreateOrder()
        {
            InitializeComponent();
            DataContext = new CreateOrderViewModel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO (backend): zapis zamowienia do bazy
            // - walidacja pol formularza
            // - utworzenie lub pobranie klienta
            // - utworzenie SalesOrder + Vehicle + VehicleFeature
            // - zapis przez UnitOfWork
            MessageBox.Show(
                "Zapis zamówienia zostanie zaimplementowany po stronie backendu.",
                "Dodaj zamówienie",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: nawigacja powrotna albo wyczyszczenie formularza
            DataContext = new CreateOrderViewModel();
        }
    }
}
