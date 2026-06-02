using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class VehiclesPage : Page
    {
        private readonly VehiclesPageViewModel _vm;

        public VehiclesPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<VehiclesPageViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Wyszukiwarka pojazdów",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) =>
            {
                await _vm.LoadFiltersAsync();
                await _vm.LoadVehiclesAsync();
            };
        }
    }
}
