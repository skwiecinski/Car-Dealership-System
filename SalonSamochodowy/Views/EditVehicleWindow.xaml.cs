using System;
using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy.Views
{
    public partial class EditVehicleWindow : FluentWindow
    {
        private readonly EditVehicleViewModel _vm;

        public event Action? VehicleEdited;

        public EditVehicleWindow(Entities.Vehicle vehicleToEdit, Window owner)
        {
            Owner = owner;
            InitializeComponent();
            _vm = new EditVehicleViewModel(vehicleToEdit);
            DataContext = _vm;

            _vm.CloseRequested += Close;
            _vm.VehicleEdited += () => VehicleEdited?.Invoke();

            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Błąd",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.ShowSuccess += msg => System.Windows.MessageBox.Show(
                msg, "Sukces",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            Loaded += async (s, e) => await _vm.LoadAsync();
        }
    }
}