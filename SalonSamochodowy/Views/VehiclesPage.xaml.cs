using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class VehiclesPage : Page
    {
        private readonly VehiclesPageViewModel _vm;
        private int _currentTourStep = 0;

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

            _vm.OpenAddJobRequested += item =>
            {
                var owner = Window.GetWindow(this);
                var addJobWindow = new AddJobWindow(item.VehicleID, owner);
                addJobWindow.JobAdded += async () => await _vm.LoadVehiclesAsync();
                addJobWindow.ShowDialog();
            };

            Loaded += async (s, e) =>
            {
                await _vm.LoadFiltersAsync();
                await _vm.LoadVehiclesAsync();
            };

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Zarządzanie Pojazdami" && this.IsVisible)
                {
                    m.Reply(true);
                    StartTour();
                }
            });
        }

        private void StartTour()
        {
            _currentTourStep = 0;
            ShowTourStep();
        }

        private void ShowTourStep()
        {
            TourPopup.IsOpen = false;
            switch (_currentTourStep)
            {
                case 0:
                    TourPopup.PlacementTarget = FilterPanel;
                    TourText.Text = "Krok 1/2: Użyj tego panelu, aby szybko przefiltrować dostępne i sprzedane pojazdy.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = VehicleScrollViewer;
                    TourText.Text = "Krok 2/2: Tutaj przeglądasz karty pojazdów. Możesz dodawać nowe lub zlecać usługi dla konkretnego auta.";
                    TourNextBtn.Content = "Zakończ";
                    break;
                default:
                    return;
            }
            TourPopup.IsOpen = true;
        }

        private void TourNext_Click(object sender, RoutedEventArgs e)
        {
            _currentTourStep++;
            ShowTourStep();
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