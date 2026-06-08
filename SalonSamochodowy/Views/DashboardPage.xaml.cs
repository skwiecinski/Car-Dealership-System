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
                if (m.PageName == "Dashboard" && this.IsVisible)
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
                    TourText.Text = "Krok 1/4: Tutaj widzisz liczbę nowych zamówień gotowych do przetworzenia.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = BorderVehicles;
                    TourText.Text = "Krok 2/4: To jest ogólna liczba aut na placu, gotowych od razu do sprzedaży.";
                    break;
                case 2:
                    TourPopup.PlacementTarget = BorderJobs;
                    TourText.Text = "Krok 3/4: Tyle zleceń serwisowych czeka na przydzielenie lub dokończenie.";
                    break;
                case 3:
                    TourPopup.PlacementTarget = BorderChart;
                    TourText.Text = "Krok 4/4: Interaktywny wykres przedstawiający Twoją dynamikę sprzedaży.";
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
