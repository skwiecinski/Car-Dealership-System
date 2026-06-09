using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    // Celowo bez [ObservableProperty] — właściwości ręczne żeby uniknąć
    // problemów z source generatorem przy nowym pliku w projekcie.
    public class EditClientViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

        private void Set<T>(ref T field, T value, string name)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(name);
        }

        // ── Oryginał e-mail potrzebny do wyszukania w bazie ──────────
        private readonly string _originalEmail;

        // ── Właściwości formularza ────────────────────────────────────
        private string _fullName = "";
        public string FullName
        {
            get => _fullName;
            set => Set(ref _fullName, value, nameof(FullName));
        }

        private string _phone = "";
        public string Phone
        {
            get => _phone;
            set => Set(ref _phone, value, nameof(Phone));
        }

        private string _taxId = "";
        public string TaxId
        {
            get => _taxId;
            set => Set(ref _taxId, value, nameof(TaxId));
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set => Set(ref _email, value, nameof(Email));
        }

        private bool _isCompany = false;
        public bool IsCompany
        {
            get => _isCompany;
            set => Set(ref _isCompany, value, nameof(IsCompany));
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => Set(ref _errorMessage, value, nameof(ErrorMessage));
        }

        private Visibility _errorVisibility = Visibility.Collapsed;
        public Visibility ErrorVisibility
        {
            get => _errorVisibility;
            set => Set(ref _errorVisibility, value, nameof(ErrorVisibility));
        }

        // ── Komendy ───────────────────────────────────────────────────
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // ── Eventy ───────────────────────────────────────────────────
        public event Action? SaveSucceeded;
        public event Action? CancelRequested;

        public EditClientViewModel(ClientModel client)
        {
            _originalEmail = client.Email;

            FullName = client.FullName;
            Phone = client.PhoneNumber;
            TaxId = client.TaxId == "-" ? "" : client.TaxId;
            Email = client.Email;
            IsCompany = client.IsCompany;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void ShowError(string msg)
        {
            ErrorMessage = msg;
            ErrorVisibility = Visibility.Visible;
        }

        private void ClearError()
        {
            ErrorMessage = "";
            ErrorVisibility = Visibility.Collapsed;
        }

        private async Task SaveAsync()
        {
            ClearError();

            var fn = FullName.Trim();
            var ph = Phone.Trim();
            var nip = TaxId.Trim();
            var em = Email.Trim();

            if (string.IsNullOrWhiteSpace(fn))
            {
                ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_ProvideName"));
                return;
            }
            if (IsCompany && string.IsNullOrWhiteSpace(nip))
            {
                ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_ProvideNIP"));
                return;
            }
            if (string.IsNullOrWhiteSpace(em))
            {
                ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_ProvideEmail"));
                return;
            }
            if (!IsValidEmail(em))
            {
                ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_InvalidEmail"));
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // Znajdź użytkownika po oryginalnym e-mailu
                var users = await uow.AppUsers.FindAsync(u => u.Email == _originalEmail);
                var user = users.FirstOrDefault();
                if (user == null)
                {
                    ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_UserNotFound"));
                    return;
                }

                // Sprawdź unikalność nowego e-maila (tylko jeśli zmieniony)
                if (!string.Equals(em, _originalEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await uow.AppUsers.FindAsync(u => u.Email == em);
                    if (existing.Any())
                    {
                        ShowError(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_EmailExistsAddClient"));
                        return;
                    }
                }

                // Aktualizuj AppUser
                if (IsCompany)
                {
                    user.FirstName = fn.Length > 50 ? fn.Substring(0, 50) : fn;
                    user.LastName = "";
                }
                else
                {
                    var parts = fn.Split(' ', 2);
                    user.FirstName = parts[0];
                    user.LastName = parts.Length > 1 ? parts[1] : "";
                }
                user.Email = em;
                uow.AppUsers.Update(user);

                // Znajdź i zaktualizuj Client
                var clients = await uow.Clients.FindAsync(c => c.UserID == user.UserID);
                var client = clients.FirstOrDefault();
                if (client != null)
                {
                    client.Phone = ph;
                    client.NIP = string.IsNullOrWhiteSpace(nip) ? null : nip;
                    uow.Clients.Update(client);
                }

                await uow.CompleteAsync();
                SaveSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Msg_SaveClientError"), ex.Message));
            }
        }

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