using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class CustomerPanelPage : Page
    {
        private readonly CustomerPanelViewModel _vm;
        private int _currentTourStep = 0;

        public CustomerPanelPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<CustomerPanelViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Panel klienta",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Moje Zamówienia" && this.IsVisible)
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
                    TourPopup.PlacementTarget = TabsElement;
                    TourText.Text = "Krok 1/1: Tutaj widzisz listę swoich zamówień podzieloną na trwające i zakończone.";
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
