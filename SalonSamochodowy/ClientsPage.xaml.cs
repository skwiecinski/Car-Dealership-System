using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy
{
    public partial class ClientsPage : Page
    {
        private readonly ClientsPageViewModel _vm = new ClientsPageViewModel();

        public ClientsPage()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Klienci",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.NewClientRequested += async () =>
            {
                var dialog = new AddClientWindow
                {
                    Owner = Window.GetWindow(this)
                };
                if (dialog.ShowDialog() == true)
                    await _vm.LoadFromDbAsync();
            };

            _vm.NewOrderRequested += selected =>
            {
                if (NavigationService != null)
                    NavigationService.Navigate(new CreateOrder());
            };

            _vm.EditClientRequested += async selected =>
            {
                var dialog = new EditClientWindow(selected, Window.GetWindow(this));
                if (dialog.ShowDialog() == true)
                    await _vm.LoadFromDbAsync();
            };

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();
        }
    }
}