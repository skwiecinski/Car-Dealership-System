using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class ClientsPage : Page
    {
        private readonly ClientsPageViewModel _vm;

        public ClientsPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<ClientsPageViewModel>();
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