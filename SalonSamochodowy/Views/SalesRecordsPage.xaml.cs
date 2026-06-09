using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;

namespace SalonSamochodowy.Views
{
    public partial class SalesRecordsPage : Page
    {
        private readonly SalesRecordsViewModel _vm;
        private int _currentTourStep = 0;

        public SalesRecordsPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<SalesRecordsViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg,
                "Ewidencja Sprzedaży",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadDataAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == nameof(SalesRecordsPage) && this.IsVisible)
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
                    TourPopup.PlacementTarget = FiltersSection;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesRecords_Step1");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Next");
                    break;
                case 1:
                    TourPopup.PlacementTarget = TableSection;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesRecords_Step2");
                    break;
                case 2:
                    TourPopup.PlacementTarget = SummarySection;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesRecords_Step3");
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
