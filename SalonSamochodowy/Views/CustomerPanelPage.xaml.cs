using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class CustomerPanelPage : Page
    {
        private readonly CustomerPanelViewModel _vm;

        public CustomerPanelPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<CustomerPanelViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Panel klienta",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadAsync();
        }
    }
}
