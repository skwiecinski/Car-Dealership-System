using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class ServicesPage : Page
    {
        private readonly ServicesPageViewModel _vm;
        private int _currentTourStep = 0;

        public ServicesPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<ServicesPageViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Usługi serwisowe",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == "Usługi Serwisowe" && this.IsVisible)
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
                    TourPopup.PlacementTarget = ColPending;
                    TourText.Text = "Krok 1/3: Zlecenia oczekujące na przypisanie do mechanika/montażysty.";
                    TourNextBtn.Content = "Dalej";
                    break;
                case 1:
                    TourPopup.PlacementTarget = ColInProgress;
                    TourText.Text = "Krok 2/3: Zlecenia w trakcie realizacji. Pasek postępu pokazuje zaawansowanie prac.";
                    break;
                case 2:
                    TourPopup.PlacementTarget = ColFinished;
                    TourText.Text = "Krok 3/3: Zakończone zlecenia. Pojazd może już wrócić do Panelu Sprzedaży i czekać na wydanie klientowi.";
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

        private async void JobCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServiceJob job)
                await OpenJobDetailAsync(job.JobID);
        }

        private async System.Threading.Tasks.Task OpenJobDetailAsync(int jobId)
        {
            var owner = Window.GetWindow(this);
            var detail = new ServicesDetailsWindow(jobId, owner);

            detail.StatusChanged += async () => await _vm.LoadFromDbAsync();

            detail.ShowDialog();
        }
    }
}
