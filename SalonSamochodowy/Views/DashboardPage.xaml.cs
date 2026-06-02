using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardPageViewModel _vm;

        public DashboardPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<DashboardPageViewModel>();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Dashboard",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadAsync();
        }
    }
}
