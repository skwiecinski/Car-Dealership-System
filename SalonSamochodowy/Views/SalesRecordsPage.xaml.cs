using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class SalesRecordsPage : Page
    {
        private readonly SalesRecordsViewModel _vm;

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
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
