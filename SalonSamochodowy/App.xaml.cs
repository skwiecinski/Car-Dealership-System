using System;
using System.Windows;

namespace SalonSamochodowy
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Łapacz błędów, który wyświetli komunikat zamiast cichego zamknięcia
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Błąd krytyczny: {ex.ExceptionObject}");
            };

            base.OnStartup(e);

            try
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas startu okna: {ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }
    }
}