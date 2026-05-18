using System;
using System.Windows;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Lapacz bledow, ktory wyswietli komunikat zamiast cichego zamkniecia
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Błąd krytyczny: {ex.ExceptionObject}");
            };

            // Inicjalizacja bazy danych + seedowanie podstawowych rekordow
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

            // Reczne otwarcie okna logowania (bez StartupUri w App.xaml zeby uniknac double-window)
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
