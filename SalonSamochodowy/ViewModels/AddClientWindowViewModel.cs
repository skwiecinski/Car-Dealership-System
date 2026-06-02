using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class AddClientWindowViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public AddClientWindowViewModel(IUnitOfWork uow)
        {
            _uow = uow;
        }
        [ObservableProperty] private string fullName = "";
        [ObservableProperty] private string phone = "";
        [ObservableProperty] private string taxId = "";
        [ObservableProperty] private string email = "";
        [ObservableProperty] private bool isCompany = false;

        public ClientModel? Result { get; private set; }

        public event Action<string>? ShowWarning;
        public event Action<string>? ShowError;
        public event Action? SaveSucceeded;
        public event Action? CancelRequested;

        [RelayCommand]
        private async Task SaveAsync()
        {
            var fn = FullName.Trim();
            var ph = Phone.Trim();
            var nip = TaxId.Trim();
            var em = Email.Trim();

            if (string.IsNullOrWhiteSpace(fn))
            {
                ShowWarning?.Invoke("Podaj imię i nazwisko lub nazwę firmy.");
                return;
            }
            if (IsCompany && string.IsNullOrWhiteSpace(nip))
            {
                ShowWarning?.Invoke("Dla klienta firmowego NIP jest wymagany.");
                return;
            }
            if (string.IsNullOrWhiteSpace(em))
            {
                ShowWarning?.Invoke("E-mail jest wymagany.");
                return;
            }
            if (!IsValidEmail(em))
            {
                ShowWarning?.Invoke("Podany adres e-mail jest niepoprawny.");
                return;
            }

            try
            {
                var uow = _uow;

                var existingUsers = await uow.AppUsers.FindAsync(u => u.Email == em);
                if (existingUsers.Any())
                {
                    ShowWarning?.Invoke("Użytkownik z takim adresem e-mail już istnieje.");
                    return;
                }

                var roleKlient = (await uow.AppRoles.FindAsync(r => r.RoleName == "Klient")).FirstOrDefault();
                if (roleKlient == null)
                {
                    ShowError?.Invoke("Rola 'Klient' nie istnieje w bazie. Zgłoś to backendowi.");
                    return;
                }

                string firstName, lastName;
                if (IsCompany)
                {
                    firstName = fn.Length > 50 ? fn.Substring(0, 50) : fn;
                    lastName  = "";
                }
                else
                {
                    var parts = fn.Split(' ', 2);
                    firstName = parts[0];
                    lastName  = parts.Length > 1 ? parts[1] : "";
                }

                var newUser = new AppUser
                {
                    FirstName    = firstName,
                    LastName     = lastName,
                    Email        = em,
                    PasswordHash = "",
                    RoleID       = roleKlient.RoleID,
                    BirthDate    = DateTime.Today
                };
                await uow.AppUsers.AddAsync(newUser);
                await uow.CompleteAsync();

                var newClient = new Client
                {
                    UserID = newUser.UserID,
                    NIP    = string.IsNullOrWhiteSpace(nip) ? null : nip,
                    Phone  = string.IsNullOrWhiteSpace(ph) ? "" : ph
                };
                await uow.Clients.AddAsync(newClient);
                await uow.CompleteAsync();

                Result = new ClientModel
                {
                    FullName    = fn,
                    PhoneNumber = ph,
                    TaxId       = string.IsNullOrWhiteSpace(nip) ? "-" : nip,
                    Email       = em,
                    IsCompany   = IsCompany
                };

                SaveSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke($"Nie udało się zapisać klienta do bazy:\n{ex.Message}\n\n{ex.InnerException?.Message}");
            }
        }

        [RelayCommand]
        private void Cancel() => CancelRequested?.Invoke();

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
