using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class SalesPanelPage : Page
    {
        private readonly SalesPanelViewModel _vm;
        private int _currentTourStep = 0;

        public SalesPanelPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<SalesPanelViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, 
                "Błąd Panelu Sprzedaży",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.OperationCompleted += msg => System.Windows.MessageBox.Show(
                msg, 
                "Operacja Zakończona",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            Loaded += async (s, e) => await _vm.LoadDataAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Trwające Sprzedaże" && this.IsVisible)
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
                    TourPopup.PlacementTarget = ColReserved;
                    TourText.Text = "Krok 1/3: Tutaj pojawiają się pojazdy dopiero co zarezerwowane przez klientów.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = ColServicing;
                    TourText.Text = "Krok 2/3: Jeśli klient zażyczył sobie usług (np. oklejenie), pojazd trafia tutaj do serwisu.";
                    break;
                case 2:
                    TourPopup.PlacementTarget = ColReady;
                    TourText.Text = "Krok 3/3: Kiedy wszystkie usługi są wykonane, pojazd czeka tu na sfinalizowanie odbioru.";
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
