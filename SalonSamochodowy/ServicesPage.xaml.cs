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

            // Dane testowe mapowane na tabelę Job
            PendingJobs = new ObservableCollection<ServiceJob>
            {
                new ServiceJob { TaskName = "Montaż alarmu", CarModel = "BMW X5 (VIN: ...456)", WorkerName = "Marek Serwisowy" },
                new ServiceJob { TaskName = "Instalacja czujników", CarModel = "Audi A6 (VIN: ...789)", WorkerName = "Jan Naprawski" }
            };

            InProgressJobs = new ObservableCollection<ServiceJob>
            {
                new ServiceJob { TaskName = "Powłoka ceramiczna", CarModel = "Mercedes GLC (VIN: ...123)", WorkerName = "Marek Serwisowy" }
            };

            FinishedJobs = new ObservableCollection<ServiceJob>
            {
                new ServiceJob { TaskName = "Przegląd przedsprzedażowy", CarModel = "BMW 3 (VIN: ...001)", WorkerName = "Jan Naprawski" }
            };

            DataContext = this;
        }
    }

    public class ServiceJob
    {
        public string TaskName { get; set; }  // FeatureID / FeatureName
        public string CarModel { get; set; }  // VehicleID
        public string WorkerName { get; set; } // WorkerID
    }
}