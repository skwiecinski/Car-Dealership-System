using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy
{
    public partial class VehiclesPage : Page
    {
        private readonly VehiclesPageViewModel _vm = new VehiclesPageViewModel();

        public VehiclesPage()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Wyszukiwarka pojazdów",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            // VM sygnalizuje chęć otwarcia okna — View otwiera je z właściwym ownerem
            _vm.OpenAddVehicleRequested += OpenAddVehicleWindow;

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
    }
}