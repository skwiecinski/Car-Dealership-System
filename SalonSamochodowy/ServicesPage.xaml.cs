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
            Loaded += async (s, e) => await _vm.LoadFromDbAsync();
        }
            /// <summary>
            /// Wywo³ywane przez MouseDoubleClick lub MouseLeftButtonUp na karcie joba
            /// (podepnij w XAML do ListBox/ItemsControl jako handler).
            /// </summary>
        private async void JobCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServiceJob job)
                await OpenJobDetailAsync(job.JobID);
        }

        /// <summary>
        /// Wariant dla przycisku / innego eventu bez MouseButtonEventArgs.
        /// </summary>
        private async void JobCard_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ServiceJob job)
                await OpenJobDetailAsync(job.JobID);
        }

        private async System.Threading.Tasks.Task OpenJobDetailAsync(int jobId)
        {
            var owner = Window.GetWindow(this);
            var detail = new ServicesDetailsWindow(jobId, owner);

            // po zmianie statusu w popupie — odœwie¿ listê
            detail.StatusChanged += async () => await _vm.LoadFromDbAsync();

            detail.ShowDialog();
        }

    }
}

