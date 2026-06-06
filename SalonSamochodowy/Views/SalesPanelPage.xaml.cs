using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class SalesPanelPage : Page
    {
        private readonly SalesPanelViewModel _vm;

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
        }
    }
}
