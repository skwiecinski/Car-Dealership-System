using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SalonSamochodowy.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using SalonSamochodowy.Messages;
using Wpf.Ui.Controls;

namespace SalonSamochodowy.Views
{
    public partial class ReportsPage : System.Windows.Controls.Page
    {
        private int _currentTourStep = 0;

        public ReportsPage()
        {
            InitializeComponent();
            var vm = ((App)Application.Current).Services.GetRequiredService<ReportsPageViewModel>();
            DataContext = vm;
            
            this.Loaded += async (s, e) => 
            {
                await vm.LoadDataAsync();
            };

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == nameof(ReportsPage) && this.IsVisible)
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
                    TourPopup.PlacementTarget = ReportsPanel;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Reports_Step1");
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
