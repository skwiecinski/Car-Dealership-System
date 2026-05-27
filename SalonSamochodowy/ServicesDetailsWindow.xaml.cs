using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class ServicesDetailsWindow : FluentWindow
    {
        private readonly ServicesDetailsViewModel _vm;

        public ServicesDetailsWindow(int jobId, Window? owner = null)
        {
            InitializeComponent();

            _vm = new ServicesDetailsViewModel(jobId);
            DataContext = _vm;

            if (owner != null)
                Owner = owner;

            _vm.ShowError += msg => System.Windows.MessageBox.Show(
                msg, "Błąd",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Error);

            _vm.ShowSuccess += msg => System.Windows.MessageBox.Show(
                msg, "Zlecenie",
                System.Windows.MessageBoxButton.OK,
                MessageBoxImage.Information);

            _vm.CloseRequested += () => Close();

            Loaded += async (s, e) => await _vm.LoadAsync();
        }

        /// <summary>
        /// Expose StatusChanged event so ServicesPage can reload after status update.
        /// </summary>
        public event Action? StatusChanged
        {
            add => _vm.StatusChanged += value;
            remove => _vm.StatusChanged -= value;
        }
    }
}