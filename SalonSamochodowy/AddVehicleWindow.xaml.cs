using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SalonSamochodowy
{
    public partial class AddVehicleWindow : Window
    {
        private readonly ViewModels.AddVehicleViewModel _viewModel;

        public AddVehicleWindow()
        {
            InitializeComponent();
            _viewModel = new ViewModels.AddVehicleViewModel();
            DataContext = _viewModel;

            this.Loaded += AddVehicleWindow_Loaded;
        }

        private async void AddVehicleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadFromDbAsync();
        }
    }
}
