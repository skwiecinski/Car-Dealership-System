using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class AdminPage : Page
    {
        private readonly AdminPageViewModel _vm;
        private int _currentTourStep = 0;

        public AdminPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<AdminPageViewModel>();
            DataContext = _vm;

            _vm.ShowMessage += (msg, img) => System.Windows.MessageBox.Show(
                msg, "Administracja",
                System.Windows.MessageBoxButton.OK,
                img);

            _vm.ConfirmDelete += msg =>
            {
                var res = System.Windows.MessageBox.Show(
                    msg, "Usuń pracownika",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);
                return res == MessageBoxResult.Yes;
            };

            _vm.GetPassword += () => TxtPassword.Password;
            _vm.ClearPassword += () => TxtPassword.Password = "";

            _vm.GetLoggedInUserId += () =>
            {
                var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                return main?.LoggedInUser?.UserID ?? -1;
            };

            Loaded += async (s, e) => await _vm.LoadAllAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == nameof(AdminPage) && this.IsVisible)
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
                    TourPopup.PlacementTarget = AdminTabs;
                    TourText.Text = "Krok 1/1: Tutaj jako administrator możesz zarządzać użytkownikami, salonami oraz słownikami systemowymi.";
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
