using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;

namespace SalonSamochodowy.Views
{
    public partial class CreateOrder : Page
    {
        private readonly CreateOrderViewModel _vm;
        private int _currentTourStep = 0;

        public CreateOrder()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<CreateOrderViewModel>();
            DataContext = _vm;

            _vm.ShowInfo += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.ShowWarning += msg => System.Windows.MessageBox.Show(
                msg, "Dodaj zamówienie",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Dodaj zamówienie" && this.IsVisible)
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
                    TourPopup.PlacementTarget = CardClient;
                    TourText.Text = "Krok 1/3: Najpierw wybierz klienta z bazy lub wpisz dane nowego klienta.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = CardVehicle;
                    TourText.Text = "Krok 2/3: Następnie wybierz markę, model i parametry techniczne pojazdu.";
                    break;
                case 2:
                    TourPopup.PlacementTarget = CardConditions;
                    TourText.Text = "Krok 3/3: Na koniec ustal datę, status, formę płatności i ewentualne usługi (np. montaż wyposażenia).";
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
