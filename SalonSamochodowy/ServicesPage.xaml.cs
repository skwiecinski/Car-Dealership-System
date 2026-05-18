using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SalonSamochodowy
{
    public partial class ServicesPage : Page
    {
        public ObservableCollection<ServiceJob> PendingJobs { get; set; }
        public ObservableCollection<ServiceJob> InProgressJobs { get; set; }
        public ObservableCollection<ServiceJob> FinishedJobs { get; set; }

        public ServicesPage()
        {
            InitializeComponent();

            PendingJobs    = new ObservableCollection<ServiceJob>();
            InProgressJobs = new ObservableCollection<ServiceJob>();
            FinishedJobs   = new ObservableCollection<ServiceJob>();

            DataContext = this;
        }
    }

    public class ServiceJob
    {
        public string TaskName { get; set; }   // FeatureID / FeatureName
        public string CarModel { get; set; }   // VehicleID
        public string WorkerName { get; set; } // WorkerID
        public int Progress { get; set; }      // 0-100 dla pracy w toku
    }
}