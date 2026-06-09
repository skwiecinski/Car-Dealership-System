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
                msg, SalonSamochodowy.Services.LocalizationHelper.GetString("Clients_Title"),
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
                string msgFmt = SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_ConfirmDeleteClient");
                string title = SalonSamochodowy.Services.LocalizationHelper.GetString("Clients_Delete");
                var result = System.Windows.MessageBox.Show(
                    string.Format(msgFmt, selected.FullName), 
                    title, 
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
                if (m.PageName == nameof(ClientsPage) && this.IsVisible)
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
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Clients_Step1");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Next");
                    break;
                case 1:
                    TourPopup.PlacementTarget = ClientsTable;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Clients_Step2");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Finish");
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