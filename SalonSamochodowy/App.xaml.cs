using System;
using System.Windows;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Błąd krytyczny: {ex.ExceptionObject}");
            };

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();
                    DbSeeder.Seed(context);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas inicjalizacji bazy danych:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }

            base.OnStartup(e);

            try
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas startu okna logowania:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }
    }
}
