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
                if (m.PageName == nameof(CreateOrder) && this.IsVisible)
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
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_CreateOrder_Step1");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Next");
                    break;
                case 1:
                    TourPopup.PlacementTarget = CardVehicle;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_CreateOrder_Step2");
                    break;
                case 2:
                    TourPopup.PlacementTarget = CardConditions;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_CreateOrder_Step3");
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
