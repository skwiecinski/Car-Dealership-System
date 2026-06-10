using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;

namespace SalonSamochodowy.ViewModels
{
    public partial class FirstConfigViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        public FirstConfigViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [ObservableProperty]
        private string adminFirstName = "";

        [ObservableProperty]
        private string adminLastName = "";

        [ObservableProperty]
        private string adminEmail = "";

        [ObservableProperty]
        private DateTime adminBirthDate = DateTime.Now.AddYears(-30);

        [ObservableProperty]
        private string dealershipName = "";

        [ObservableProperty]
        private string dealershipAddress = "";

        [ObservableProperty]
        private string dealershipCity = "";

        [ObservableProperty]
        private string dealershipOwner = "";

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private bool isLoading;

        public event Action? ConfigurationCompleted;

        [RelayCommand]
        private async Task SaveConfigAsync(object? passwordSource)
        {
            ErrorMessage = "";

            var password = (passwordSource as Wpf.Ui.Controls.PasswordBox)?.Password ?? "";

            if (string.IsNullOrWhiteSpace(AdminFirstName) ||
                string.IsNullOrWhiteSpace(AdminLastName) ||
                string.IsNullOrWhiteSpace(AdminEmail) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(DealershipName) ||
                string.IsNullOrWhiteSpace(DealershipAddress) ||
                string.IsNullOrWhiteSpace(DealershipCity) ||
                string.IsNullOrWhiteSpace(DealershipOwner))
            {
                ErrorMessage = "Wszystkie pola są wymagane.";
                return;
            }

            if (!IsValidEmail(AdminEmail.Trim()))
            {
                ErrorMessage = "Podany adres e-mail jest niepoprawny.";
                return;
            }

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    
                    if (!context.AppRoles.Any())
                    {
                        context.AppRoles.AddRange(
                            new AppRole { RoleName = RoleNames.Admin, PermissionLevel = 0 },
                            new AppRole { RoleName = RoleNames.Kierownik, PermissionLevel = 1 },
                            new AppRole { RoleName = RoleNames.Sprzedawca, PermissionLevel = 2 },
                            new AppRole { RoleName = RoleNames.Serwisant, PermissionLevel = 3 },
                            new AppRole { RoleName = RoleNames.Klient, PermissionLevel = 4 }
                        );
                        context.SaveChanges();
                    }

                    var adminRole = context.AppRoles.First(r => r.RoleName == RoleNames.Admin);

                    
                    var dealership = new Dealership
                    {
                        Name = DealershipName.Trim(),
                        Address = DealershipAddress.Trim(),
                        City = DealershipCity.Trim(),
                        Owner = DealershipOwner.Trim()
                    };
                    context.Dealerships.Add(dealership);
                    context.SaveChanges();

                    
                    var user = new AppUser
                    {
                        FirstName = AdminFirstName.Trim(),
                        LastName = AdminLastName.Trim(),
                        Email = AdminEmail.Trim(),
                        PasswordHash = AuthService.HashPassword(password),
                        RoleID = adminRole.RoleID,
                        BirthDate = AdminBirthDate
                    };
                    context.AppUsers.Add(user);
                    context.SaveChanges();

                    var worker = new Worker
                    {
                        UserID = user.UserID,
                        DealershipID = dealership.DealershipID,
                        Payroll = 0,
                        EndOfContractDate = DateTime.Now.AddYears(10)
                    };
                    context.Workers.Add(worker);
                    context.SaveChanges();
                });

                ConfigurationCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Wystąpił błąd podczas zapisu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var address))
                return false;

            var domain = address.Host;
            var dotIndex = domain.IndexOf('.');
            return dotIndex > 0 && dotIndex < domain.Length - 1;
        }
    }
}
