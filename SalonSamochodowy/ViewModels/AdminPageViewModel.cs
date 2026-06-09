using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;
using SalonSamochodowy.Services;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace SalonSamochodowy.ViewModels
{
    public partial class AdminPageViewModel : ObservableObject
    {
        private readonly IUnitOfWork _uow;

        public AdminPageViewModel(IUnitOfWork uow)
        {
            _uow = uow;
            WeakReferenceMessenger.Default.Register(this, (AdminPageViewModel r, SalonSamochodowy.Messages.LanguageChangedMessage m) =>
            {
                var tempAccounts = r.Accounts.ToList();
                r.Accounts.Clear();
                foreach (var a in tempAccounts) r.Accounts.Add(a);
            });
        }
        public ObservableCollection<AccountRow> Accounts { get; } = new();
        public ObservableCollection<DealershipRow> Dealerships { get; } = new();
        public ObservableCollection<DealershipItem> AvailableDealerships { get; } = new();

        [ObservableProperty] private string firstName = "";
        [ObservableProperty] private string lastName = "";
        [ObservableProperty] private string email = "";
        [ObservableProperty] private string roleName = RoleNames.Sprzedawca;
        [ObservableProperty] private DealershipItem? selectedDealershipForEmployee;
        [ObservableProperty] private string accountsCountText = "";

        [ObservableProperty] private string newSalonName = "";
        [ObservableProperty] private string newSalonAddress = "";
        [ObservableProperty] private string newSalonCity = "";
        [ObservableProperty] private string newSalonOwner = "";
        [ObservableProperty] private string dealershipsCountText = "";

        public event Action<string, MessageBoxImage>? ShowMessage;
        public event Func<string, bool>? ConfirmDelete;
        public event Func<string>? GetPassword;
        public event Action? ClearPassword;
        public event Func<int>? GetLoggedInUserId;

        public async Task LoadAllAsync()
        {
            await LoadDealershipsAsync();
            await LoadAccountsAsync();
            await LoadDictionariesAsync();
        }

        public ObservableCollection<VehicleModel> DictModels { get; } = new();
        public ObservableCollection<Feature> DictFeatures { get; } = new();
        public ObservableCollection<Engine> DictEngines { get; } = new();
        public ObservableCollection<TrimLevel> DictTrims { get; } = new();

        public string[] FeatureCategories { get; } = new[] { "Wygląd", "Akcesoria", "Bezpieczeństwo", "Komfort", "Wnętrze", "Multimedia", "Usługa" };

        [ObservableProperty] private string newModelBrand = "";
        [ObservableProperty] private string newModelName = "";

        [ObservableProperty] private string newFeatureName = "";
        [ObservableProperty] private string newFeatureCategory = "Akcesoria";
        [ObservableProperty] private decimal? newFeaturePrice;

        [ObservableProperty] private string newEngineBrand = "";
        [ObservableProperty] private string newEngineName = "";
        [ObservableProperty] private string newEngineSize = "";
        [ObservableProperty] private int? newEnginePower;
        [ObservableProperty] private decimal? newEnginePrice;

        [ObservableProperty] private VehicleModel? newTrimSelectedModel;
        [ObservableProperty] private string newTrimName = "";
        [ObservableProperty] private decimal? newTrimBasePrice;

        public async Task LoadDictionariesAsync()
        {
            try
            {
                var uow = _uow;

                DictModels.Clear();
                var models = await uow.VehicleModels.GetAllAsync();
                foreach (var m in models.OrderBy(x => x.Brand).ThenBy(x => x.ModelName))
                    DictModels.Add(m);

                DictFeatures.Clear();
                var features = await uow.Features.GetAllAsync();
                foreach (var f in features.OrderBy(x => x.Category).ThenBy(x => x.FeatureName))
                    DictFeatures.Add(f);

                DictEngines.Clear();
                var engines = await uow.Engines.GetAllAsync();
                foreach (var e in engines.OrderBy(x => x.Brand).ThenBy(x => x.Power))
                    DictEngines.Add(e);

                DictTrims.Clear();
                var trims = await uow.TrimLevels.GetAllWithIncludesAsync(t => t.Model);
                foreach (var t in trims.OrderBy(x => x.Model.Brand).ThenBy(x => x.Model.ModelName).ThenBy(x => x.BasePrice))
                    DictTrims.Add(t);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DictLoadError"), ex.Message), MessageBoxImage.Error);
            }
        }

        public async Task LoadAccountsAsync()
        {
            try
            {
                var uow = _uow;

                Accounts.Clear();
                var users = (await uow.AppUsers.GetAllWithIncludesAsync(u => u.Role)).ToList();
                foreach (var u in users.OrderBy(x => x.RoleID).ThenBy(x => x.LastName))
                {
                    var role = u.Role;
                    var workers = await uow.Workers.FindWithIncludesAsync(w => w.UserID == u.UserID, w => w.Dealership);
                    var worker = workers.FirstOrDefault();
                    Dealership? dealership = worker?.Dealership;

                    Accounts.Add(new AccountRow
                    {
                        UserID         = u.UserID,
                        DealershipID   = dealership?.DealershipID,
                        FullName       = $"{u.FirstName} {u.LastName}".Trim(),
                        Email          = u.Email,
                        RoleName       = role?.RoleName ?? "—",
                        DealershipName = dealership?.Name ?? "—"
                    });
                }
                AccountsCountText = string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountsCount"), Accounts.Count);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountsLoadError"), ex.Message), MessageBoxImage.Error);
            }
        }

        public async Task LoadDealershipsAsync()
        {
            try
            {
                var uow = _uow;

                var prevSelectedId = SelectedDealershipForEmployee?.DealershipID;

                Dealerships.Clear();
                AvailableDealerships.Clear();

                var list = (await uow.Dealerships.GetAllAsync()).ToList();
                foreach (var d in list.OrderBy(d => d.City).ThenBy(d => d.Name))
                {
                    Dealerships.Add(new DealershipRow
                    {
                        DealershipID = d.DealershipID,
                        Name         = d.Name,
                        Address      = d.Address,
                        City         = d.City,
                        Owner        = d.Owner
                    });
                    AvailableDealerships.Add(new DealershipItem
                    {
                        DealershipID = d.DealershipID,
                        Display      = $"{d.Name} ({d.City})"
                    });
                }
                DealershipsCountText = string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipsCount"), Dealerships.Count);

                if (prevSelectedId.HasValue)
                    SelectedDealershipForEmployee = AvailableDealerships.FirstOrDefault(x => x.DealershipID == prevSelectedId.Value);

                if (SelectedDealershipForEmployee == null && AvailableDealerships.Count > 0)
                    SelectedDealershipForEmployee = AvailableDealerships.First();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipsLoadError"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task RegisterEmployeeAsync()
        {
            var fn = FirstName.Trim();
            var ln = LastName.Trim();
            var em = Email.Trim();
            var pw = GetPassword?.Invoke() ?? "";

            if (string.IsNullOrWhiteSpace(fn) || string.IsNullOrWhiteSpace(ln))
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValName"), MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(em) || !IsValidEmail(em))
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValEmail"), MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(pw) || pw.Length < 4)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValPassword"), MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValRole"), MessageBoxImage.Warning);
                return;
            }

            var isWorkerRole = RoleName == RoleNames.Sprzedawca || RoleName == RoleNames.Serwisant || RoleName == RoleNames.Kierownik;
            if (isWorkerRole && SelectedDealershipForEmployee == null)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValDealership"), MessageBoxImage.Warning);
                return;
            }

            try
            {
                var uow = _uow;

                var existing = await uow.AppUsers.FindAsync(u => u.Email == em);
                if (existing.Any())
                {
                    ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_EmailExists"), MessageBoxImage.Warning);
                    return;
                }

                var role = (await uow.AppRoles.FindAsync(r => r.RoleName == RoleName)).FirstOrDefault();
                if (role == null)
                {
                    ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_RoleMissing"), RoleName), MessageBoxImage.Error);
                    return;
                }

                var newUser = new AppUser
                {
                    FirstName    = fn,
                    LastName     = ln,
                    Email        = em,
                    PasswordHash = AuthService.HashPassword(pw),
                    RoleID       = role.RoleID,
                    BirthDate    = DateTime.Today
                };
                await uow.AppUsers.AddAsync(newUser);
                await uow.CompleteAsync();

                if (isWorkerRole)
                {
                    await uow.Workers.AddAsync(new Worker
                    {
                        UserID            = newUser.UserID,
                        Payroll           = 0m,
                        EndOfContractDate = DateTime.Today.AddYears(2),
                        DealershipID      = SelectedDealershipForEmployee!.DealershipID
                    });
                    await uow.CompleteAsync();
                }

                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountCreated"), $"{fn} {ln}", RoleName), MessageBoxImage.Information);

                FirstName = "";
                LastName = "";
                Email = "";
                RoleName = RoleNames.Sprzedawca;
                ClearPassword?.Invoke();

                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountCreateError"), ex.Message, ex.InnerException?.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteEmployeeAsync(AccountRow? row)
        {
            if (row == null) return;

            if (row.RoleName == RoleNames.Kierownik)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteProtected"), MessageBoxImage.Warning);
                return;
            }

            var confirmMsg = string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteConfirm"), row.FullName, row.Email, row.RoleName, row.DealershipName);
            if (ConfirmDelete?.Invoke(confirmMsg) != true) return;

            try
            {
                var uow = _uow;

                var user = (await uow.AppUsers.FindAsync(u => u.Email == row.Email)).FirstOrDefault();
                if (user == null)
                {
                    ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountNotFound"), MessageBoxImage.Warning);
                    return;
                }

                var loggedId = GetLoggedInUserId?.Invoke() ?? -1;
                if (loggedId == user.UserID)
                {
                    ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_SelfDelete"), MessageBoxImage.Warning);
                    return;
                }

                var workers = await uow.Workers.FindAsync(w => w.UserID == user.UserID);
                foreach (var w in workers) uow.Workers.Delete(w);

                var clients = await uow.Clients.FindAsync(c => c.UserID == user.UserID);
                foreach (var c in clients) uow.Clients.Delete(c);

                uow.AppUsers.Delete(user);
                await uow.CompleteAsync();

                await LoadAccountsAsync();
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountDeleted"), row.FullName), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountDeleteError"), ex.Message, ex.InnerException?.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditEmployeeAsync(AccountRow? row)
        {
            if (row == null) return;

            var window = new SalonSamochodowy.Views.EditEmployeeWindow(row, AvailableDealerships, Application.Current.MainWindow);
            if (window.ShowDialog() == true)
            {
                var vm = window.ViewModel;
                try
                {
                    var uow = _uow;

                    var user = (await uow.AppUsers.FindAsync(u => u.UserID == row.UserID)).FirstOrDefault();
                    if (user == null)
                    {
                        ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountNotFound"), MessageBoxImage.Warning);
                        return;
                    }

                    var newRole = (await uow.AppRoles.FindAsync(r => r.RoleName == vm.RoleName)).FirstOrDefault();
                    if (newRole == null)
                    {
                        ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_RoleMissing"), vm.RoleName), MessageBoxImage.Error);
                        return;
                    }

                    user.FirstName = vm.FirstName;
                    user.LastName = vm.LastName;
                    user.Email = vm.Email;
                    user.RoleID = newRole.RoleID;

                    if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                    {
                        user.PasswordHash = AuthService.HashPassword(vm.NewPassword);
                    }

                    bool isNewRoleWorker = vm.RoleName == RoleNames.Sprzedawca || vm.RoleName == RoleNames.Serwisant || vm.RoleName == RoleNames.Kierownik;

                    var existingWorkers = await uow.Workers.FindAsync(w => w.UserID == user.UserID);
                    var worker = existingWorkers.FirstOrDefault();

                    if (isNewRoleWorker)
                    {
                        if (worker == null)
                        {
                            await uow.Workers.AddAsync(new Worker
                            {
                                UserID = user.UserID,
                                Payroll = 0m,
                                EndOfContractDate = DateTime.Today.AddYears(2),
                                DealershipID = vm.SelectedDealership!.DealershipID
                            });
                        }
                        else
                        {
                            worker.DealershipID = vm.SelectedDealership!.DealershipID;
                        }
                    }
                    else
                    {
                        if (worker != null)
                        {
                            uow.Workers.Delete(worker);
                        }
                    }

                    await uow.CompleteAsync();
                    await LoadAccountsAsync();
                    ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountUpdated"), $"{user.FirstName} {user.LastName}"), MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_AccountUpdateError"), ex.Message), MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task RegisterDealershipAsync()
        {
            var name    = NewSalonName.Trim();
            var address = NewSalonAddress.Trim();
            var city    = NewSalonCity.Trim();
            var owner   = NewSalonOwner.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(owner))
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValDealershipData"), MessageBoxImage.Warning);
                return;
            }

            try
            {
                var uow = _uow;

                var existing = await uow.Dealerships.FindAsync(d => d.Name == name && d.City == city);
                if (existing.Any())
                {
                    ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipExists"), name, city), MessageBoxImage.Warning);
                    return;
                }

                await uow.Dealerships.AddAsync(new Dealership
                {
                    Name    = name,
                    Address = address,
                    City    = city,
                    Owner   = owner
                });
                await uow.CompleteAsync();

                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipAdded"), name, city), MessageBoxImage.Information);

                NewSalonName    = "";
                NewSalonAddress = "";
                NewSalonCity    = "";
                NewSalonOwner   = "";

                await LoadDealershipsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipAddError"), ex.Message, ex.InnerException?.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteDealershipAsync(DealershipRow? row)
        {
            if (row == null) return;

            try
            {
                var uow = _uow;

                var workersHere = await uow.Workers.CountAsync(w => w.DealershipID == row.DealershipID);
                if (workersHere > 0)
                {
                    ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipHasWorkers"), row.Name, workersHere), MessageBoxImage.Warning);
                    return;
                }

                var confirmMsg = string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteDealershipConfirm"), row.Name, row.Address, row.City, row.Owner);
                if (ConfirmDelete?.Invoke(confirmMsg) != true) return;

                var dealership = await uow.Dealerships.GetByIdAsync(row.DealershipID);
                if (dealership == null)
                {
                    ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipNotFound"), MessageBoxImage.Warning);
                    return;
                }

                uow.Dealerships.Delete(dealership);
                await uow.CompleteAsync();

                await LoadDealershipsAsync();
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipDeleted"), row.Name), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DealershipDeleteError"), ex.Message, ex.InnerException?.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddVehicleModelAsync()
        {
            var brand = NewModelBrand.Trim();
            var name = NewModelName.Trim();
            if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(name))
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValModel"), MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateVehicleModelAsync(new VehicleModel { Brand = brand, ModelName = name });
                NewModelBrand = "";
                NewModelName = "";
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ModelAdded"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteVehicleModelAsync(VehicleModel? model)
        {
            if (model == null) return;
            if (ConfirmDelete?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteModelConfirm"), model.Brand, model.ModelName)) != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteVehicleModelAsync(model.ModelID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ModelDeleted"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddFeatureAsync()
        {
            var name = NewFeatureName.Trim();
            var cat = NewFeatureCategory;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(cat) || !NewFeaturePrice.HasValue)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValFeature"), MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateFeatureAsync(new Feature { FeatureName = name, Category = cat, Price = NewFeaturePrice.Value });
                NewFeatureName = "";
                NewFeaturePrice = null;
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_FeatureAdded"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteFeatureAsync(Feature? feature)
        {
            if (feature == null) return;
            if (ConfirmDelete?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteFeatureConfirm"), feature.FeatureName)) != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteFeatureAsync(feature.FeatureID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_FeatureDeleted"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddEngineAsync()
        {
            var brand = NewEngineBrand.Trim();
            var name = NewEngineName.Trim();
            var size = NewEngineSize.Trim();
            if (string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(name) || !NewEnginePower.HasValue || !NewEnginePrice.HasValue)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValEngine"), MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateEngineAsync(new Engine 
                { 
                    Brand = brand, EngineName = name, EngineSize = size, 
                    Power = NewEnginePower.Value, Price = NewEnginePrice.Value 
                });
                NewEngineBrand = "";
                NewEngineName = "";
                NewEngineSize = "";
                NewEnginePower = null;
                NewEnginePrice = null;
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_EngineAdded"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteEngineAsync(Engine? engine)
        {
            if (engine == null) return;
            if (ConfirmDelete?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteEngineConfirm"), engine.EngineName)) != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteEngineAsync(engine.EngineID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_EngineDeleted"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task AddTrimLevelAsync()
        {
            var name = NewTrimName.Trim();
            if (NewTrimSelectedModel == null || string.IsNullOrWhiteSpace(name) || !NewTrimBasePrice.HasValue)
            {
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_ValTrim"), MessageBoxImage.Warning);
                return;
            }
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.CreateTrimLevelAsync(new TrimLevel 
                { 
                    ModelID = NewTrimSelectedModel.ModelID, 
                    TrimName = name, 
                    BasePrice = NewTrimBasePrice.Value 
                });
                NewTrimName = "";
                NewTrimBasePrice = null;
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_TrimAdded"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTrimLevelAsync(TrimLevel? trim)
        {
            if (trim == null) return;
            if (ConfirmDelete?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_DeleteTrimConfirm"), trim.TrimName, trim.Model?.ModelName)) != true) return;
            try
            {
                var catalogService = ((App)Application.Current).Services.GetRequiredService<SalonSamochodowy.Services.ICatalogService>();
                await catalogService.DeleteTrimLevelAsync(trim.TrimID);
                await LoadDictionariesAsync();
                ShowMessage?.Invoke(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_TrimDeleted"), MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(string.Format(SalonSamochodowy.Services.LocalizationHelper.GetString("Admin_Error"), ex.Message), MessageBoxImage.Error);
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

    public class AccountRow
    {
        public int UserID            { get; set; }
        public int? DealershipID     { get; set; }
        public string FullName       { get; set; } = "";
        public string Email          { get; set; } = "";
        public string RoleName       { get; set; } = "";
        public string DealershipName { get; set; } = "";
        public string DisplayRoleName => RoleName == "—" ? "—" : SalonSamochodowy.Services.LocalizationHelper.GetString($"Role_{RoleName}");
    }

    public class DealershipRow
    {
        public int    DealershipID { get; set; }
        public string Name         { get; set; } = "";
        public string Address      { get; set; } = "";
        public string City         { get; set; } = "";
        public string Owner        { get; set; } = "";
    }

    public class DealershipItem
    {
        public int    DealershipID { get; set; }
        public string Display      { get; set; } = "";
        public override string ToString() => Display;
    }
}
