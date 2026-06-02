using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class ServicesPage : Page
    {
        private readonly ServicesPageViewModel _vm;

        public ServicesPage()
        {
            InitializeComponent();
            _vm = ((App)Application.Current).Services.GetRequiredService<ServicesPageViewModel>();
            DataContext = _vm;
            Loaded += async (s, e) => await _vm.LoadFromDbAsync();
        }

        private async void JobCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServiceJob job)
                await OpenJobDetailAsync(job.JobID);
        }

        private async void JobCard_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServiceJob job)
                await OpenJobDetailAsync(job.JobID);
        }

        private async System.Threading.Tasks.Task OpenJobDetailAsync(int jobId)
        {
            var owner = Window.GetWindow(this);
            var detail = new ServicesDetailsWindow(jobId, owner);

            detail.StatusChanged += async () => await _vm.LoadFromDbAsync();

            detail.ShowDialog();
        }

    }
}

