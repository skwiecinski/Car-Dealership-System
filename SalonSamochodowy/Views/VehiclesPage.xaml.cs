using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            _vm.OpenAddVehicleRequested += OpenAddVehicleWindow;

            _vm.DeleteVehicleRequested += async item =>
            {
                var result = System.Windows.MessageBox.Show(
                    $"Czy na pewno chcesz usunąć pojazd {item.FullName} ({item.VIN})?", 
                    "Usuń pojazd", 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Warning);
                    
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var vehicleService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.IVehicleService>();
                    var v = await vehicleService.GetVehicleByIdAsync(item.VehicleID);
                    if (v != null)
                    {
                        await vehicleService.DeleteVehicleAsync(v);
                        await _vm.LoadVehiclesAsync();
                    }
                }
            };

            _vm.EditVehicleRequested += async item =>
            {
                var owner = Window.GetWindow(this);
                var vehicleService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.IVehicleService>();
                var v = await vehicleService.GetVehicleByIdAsync(item.VehicleID);
                if (v != null)
                {
                    var editWindow = new EditVehicleWindow(v, owner);
                    editWindow.VehicleEdited += async () => await _vm.LoadVehiclesAsync();
                    editWindow.ShowDialog();
                }
            };

            Loaded += async (s, e) =>
            {
                await _vm.LoadFiltersAsync();
                await _vm.LoadVehiclesAsync();
            };
        }

        private async void OpenAddVehicleWindow()
        {
            var owner = Window.GetWindow(this);
            var addWindow = new AddVehicleWindow(owner);

            addWindow.VehicleAdded += async () => await _vm.LoadVehiclesAsync();

            addWindow.ShowDialog();
        }

        private void VehicleScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}