using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy
{
    public partial class ServicesPage : Page
    {
        private readonly ServicesPageViewModel _vm;

        public ServicesPage()
        {
            InitializeComponent();
            _vm = new ServicesPageViewModel();
            DataContext = _vm;

            _vm.LoadFailed += msg => System.Windows.MessageBox.Show(
                msg, "Us³ugi serwisowe",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Loaded += async (s, e) => await _vm.LoadFromDbAsync();
        }

        private async void JobCard_Click(object sender, MouseButtonEventArgs e)
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