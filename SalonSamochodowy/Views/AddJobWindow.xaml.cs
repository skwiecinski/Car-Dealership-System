using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class AddJobWindow : Window
    {
        private readonly AddJobViewModel _vm;
        public event Action? JobAdded;

        public AddJobWindow(int vehicleId, Window owner)
        {
            InitializeComponent();
            Owner = owner;
            _vm = ((App)Application.Current).Services.GetRequiredService<AddJobViewModel>();
            DataContext = _vm;

            _vm.RequestClose += Close;
            _vm.ShowError += msg => MessageBox.Show(msg, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            _vm.JobAdded += () => JobAdded?.Invoke();

            Loaded += async (s, e) => await _vm.InitializeAsync(vehicleId);
        }
    }
}
