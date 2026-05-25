using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class ServicesPageViewModel : ObservableObject
    {
        public ObservableCollection<ServiceJob> PendingJobs { get; } = new();
        public ObservableCollection<ServiceJob> InProgressJobs { get; } = new();
        public ObservableCollection<ServiceJob> FinishedJobs { get; } = new();
        public async Task LoadFromDbAsync()
        {

            /*
             zaœ tu trzeba zajebaæ trochu rozkminki
            zebrac userID zalogowanego
            bo kierownik musi widziec wszystkie zlecenia jego mrówek
            a mrówka tylko swój
             */

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                if(loggedInUserRole == "Kierownik")
                {

                }
                else if(loggedInUserRole == "Serwisant")
                {

                }

                    var allJobs = await uow.Jobs.GetAllAsync();

                foreach (var job in allJobs)
                {
                    String jobStatus = job.Status;


                }
            }
            catch (Exception ex)
            {
            }

        }
    }
    public class ServiceJob
    {
        public string TaskName { get; set; } = "";
        public string CarModel { get; set; } = "";
        public string WorkerName { get; set; } = "";
        public int Progress { get; set; }
    }
}
