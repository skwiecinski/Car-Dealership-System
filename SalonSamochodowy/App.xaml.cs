using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.ViewModels;
using SalonSamochodowy.Views;

namespace SalonSamochodowy
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Database
            services.AddTransient<AppDbContext>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<SalonSamochodowy.Services.AuthService>();
            services.AddTransient<SalonSamochodowy.Services.IVehicleService, SalonSamochodowy.Services.VehicleService>();
            services.AddTransient<SalonSamochodowy.Services.IOrderService, SalonSamochodowy.Services.OrderService>();
            services.AddTransient<SalonSamochodowy.Services.IClientService, SalonSamochodowy.Services.ClientService>();
            services.AddTransient<SalonSamochodowy.Services.IJobService, SalonSamochodowy.Services.JobService>();
            services.AddTransient<SalonSamochodowy.Services.ICatalogService, SalonSamochodowy.Services.CatalogService>();

            // ViewModels
            services.AddTransient<AddClientWindowViewModel>();
            services.AddTransient<AdminPageViewModel>();
            services.AddTransient<ClientsPageViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<DashboardPageViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ServicesDetailsViewModel>();
            services.AddTransient<ServicesPageViewModel>();
            services.AddTransient<VehiclesPageViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Błąd krytyczny: {ex.ExceptionObject}");
            };

            try
            {
                using (var scope = Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
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
