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
            services.AddTransient<SalonSamochodowy.Services.ReportGeneratorService>();

            // ViewModels
            services.AddTransient<AddClientWindowViewModel>();
            services.AddTransient<AddJobViewModel>();
            services.AddTransient<AdminPageViewModel>();
            services.AddTransient<ClientsPageViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<CustomerPanelViewModel>();
            services.AddTransient<DashboardPageViewModel>();
            services.AddTransient<FirstConfigViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<SalesPanelViewModel>();
            services.AddTransient<SalesRecordsViewModel>();
            services.AddTransient<ServicesDetailsViewModel>();
            services.AddTransient<ServicesPageViewModel>();
            services.AddTransient<VehiclesPageViewModel>();
            services.AddTransient<ReportsPageViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Błąd krytyczny: {ex.ExceptionObject}");
            };

            bool isFirstRun = false;

            try
            {
                using (var scope = Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    context.Database.EnsureCreated();
                    
                    // Sprawdzamy czy to pierwsze uruchomienie (brak jakichkolwiek użytkowników)
                    if (!context.AppUsers.Any())
                    {
                        isFirstRun = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas inicjalizacji bazy danych:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }

            base.OnStartup(e);

            try
            {
                if (isFirstRun)
                {
                    var firstConfigWindow = new FirstConfigWindow();
                    firstConfigWindow.Show();
                }
                else
                {
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas startu aplikacji:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }
    }
}
