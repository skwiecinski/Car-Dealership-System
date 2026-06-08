using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy.Views
{
    public partial class ReportsPage : System.Windows.Controls.Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            var vm = ((App)Application.Current).Services.GetRequiredService<ReportsPageViewModel>();
            DataContext = vm;
            
            this.Loaded += async (s, e) => 
            {
                await vm.LoadDataAsync();
            };
        }
    }
}
