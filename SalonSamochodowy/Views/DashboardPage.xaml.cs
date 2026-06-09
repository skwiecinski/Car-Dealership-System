using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardPageViewModel _vm;
        private int _currentTourStep = 0;

        public DashboardPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<DashboardPageViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Dashboard",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == nameof(DashboardPage) && this.IsVisible)
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
                    TourPopup.PlacementTarget = BorderOrders;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Dash_Step1");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Next");
                    break;
                case 1:
                    TourPopup.PlacementTarget = BorderVehicles;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Dash_Step2");
                    break;
                case 2:
                    TourPopup.PlacementTarget = BorderJobs;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Dash_Step3");
                    break;
                case 3:
                    TourPopup.PlacementTarget = BorderChart;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Dash_Step4");
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

        private void SeeAllOrders_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;
            
            var user = mainWindow.LoggedInUser;
            if (user?.Role?.RoleName == "Klient")
                mainWindow.RootNavigation.Navigate(typeof(CustomerPanelPage));
            else
                mainWindow.RootNavigation.Navigate(typeof(SalesPanelPage));
        }
    }
}
