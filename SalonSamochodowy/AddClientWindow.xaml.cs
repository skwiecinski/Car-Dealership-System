using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class AddClientWindow : FluentWindow
    {
        private readonly AddClientViewModel _vm = new AddClientViewModel();

        public ClientModel? Result => _vm.Result;

        public AddClientWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.ShowWarning += msg => System.Windows.MessageBox.Show(
                msg, "Nowy klient",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Nowy klient",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            _vm.SaveSucceeded += () =>
            {
                DialogResult = true;
                Close();
            };

            _vm.CancelRequested += () =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}
