using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class ClientsPage : Page
    {
        private readonly ClientsPageViewModel _vm;
        private int _currentTourStep = 0;

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

            _vm.DeleteClientRequested += async selected =>
            {
                var result = System.Windows.MessageBox.Show(
                    $"Czy na pewno chcesz usunąć klienta {selected.FullName}?", 
                    "Usuń klienta", 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Warning);
                    
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var clientService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.IClientService>();
                    await clientService.DeleteClientAsync(selected.ClientId);
                    await _vm.LoadFromDbAsync();
                }
            };

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Klienci i Sprzedaż" && this.IsVisible)
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
                    TourPopup.PlacementTarget = BtnNewClient;
                    TourText.Text = "Krok 1/2: Kliknij tutaj, aby dodać nowego klienta do bazy.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = ClientsTable;
                    TourText.Text = "Krok 2/2: W tej tabeli znajdziesz wszystkich zapisanych klientów. Po kliknięciu na wiersz zobaczysz opcje zarządzania po prawej stronie.";
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
    }
}