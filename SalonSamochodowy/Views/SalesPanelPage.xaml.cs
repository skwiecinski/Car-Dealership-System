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
                SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_ErrorTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.OperationCompleted += msg => System.Windows.MessageBox.Show(
                msg, 
                SalonSamochodowy.Services.LocalizationHelper.GetString("SalesPanel_SuccessTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            Loaded += async (s, e) => await _vm.LoadDataAsync();

            WeakReferenceMessenger.Default.Register<StartTourRequestMessage>(this, (r, m) =>
            {
                if (m.PageName == nameof(SalesPanelPage) && this.IsVisible)
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
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesPanel_Step1");
                    TourNextBtn.Content = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_Next");
                    break;
                case 1:
                    TourPopup.PlacementTarget = ColServicing;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesPanel_Step2");
                    break;
                case 2:
                    TourPopup.PlacementTarget = ColReady;
                    TourText.Text = SalonSamochodowy.Services.LocalizationHelper.GetString("Tour_SalesPanel_Step3");
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
