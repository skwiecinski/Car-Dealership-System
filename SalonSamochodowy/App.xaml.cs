using System.Windows;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            using (var context = new AppDbContext())
            {
                // zapewnia ze plik .db istnieje
                context.Database.EnsureCreated();
                // bazowe dane - po jednym rekordzie na kazda tabele
                // lokalizacja: Entities/DbSeeder
                DbSeeder.Seed(context);
            }

            base.OnStartup(e);
        }
    }
}