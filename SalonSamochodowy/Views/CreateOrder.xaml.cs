using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace SalonSamochodowy.Views
{
    public partial class CreateOrder : Page
    {
        private readonly CreateOrderViewModel _vm;

        public CreateOrder()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<CreateOrderViewModel>();
            DataContext = _vm;

            _vm.ShowInfo += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.ShowWarning += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();
        }
    }
}
