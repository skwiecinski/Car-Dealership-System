using System;
using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy.Views
{
    public partial class AddVehicleWindow : FluentWindow
    {
        private readonly AddVehicleViewModel _vm;

        public AddVehicleWindow(Window? owner = null)
        {
            InitializeComponent();

            _vm = new AddVehicleViewModel();
            DataContext = _vm;

            if (owner != null)
            {
                this.Owner = owner;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            
            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Błąd",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Error);

            _vm.ShowSuccess += msg => System.Windows.MessageBox.Show(
                msg, "Dodawanie pojazdu",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Information);

            _vm.CloseRequested += () => Close();

            Loaded += async (s, e) => await _vm.LoadAsync();
        }

        public event Action? VehicleAdded
        {
            add => _vm.VehicleAdded += value;
            remove => _vm.VehicleAdded -= value;
        }
    }
}