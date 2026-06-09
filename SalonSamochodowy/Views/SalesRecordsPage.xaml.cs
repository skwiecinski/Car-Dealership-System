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
                if (m.PageName == "Ewidencja Sprzedaży" && this.IsVisible)
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
                    TourText.Text = "Krok 1/3: Użyj tych filtrów, aby zawęzić wyniki według statusu, salonu, doradcy lub zakresu dat.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = TableSection;
                    TourText.Text = "Krok 2/3: Tutaj znajduje się pełna lista transakcji spełniających podane kryteria.";
                    break;
                case 2:
                    TourPopup.PlacementTarget = SummarySection;
                    TourText.Text = "Krok 3/3: W tym miejscu zobaczysz podsumowanie - np. łączną kwotę sfinalizowanych zamówień.";
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
