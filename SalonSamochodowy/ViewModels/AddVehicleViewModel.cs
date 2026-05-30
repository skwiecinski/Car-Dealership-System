using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories;

namespace SalonSamochodowy.ViewModels
{
    public partial class AddVehicleViewModel : ObservableObject
    {
        public Vehicle NewVehicle { get; set; }

        public ObservableCollection<Engine> AvailableEngines { get; } = new();
        public ObservableCollection<VehicleModel> AvailableModels { get; } = new();
        public ObservableCollection<TrimLevel> AvailableTrims { get; } = new();

        private List<TrimLevel> _allTrims = new();

        public Engine SelectedEngine { get; set; }

        private VehicleModel _selectedModel;
        public VehicleModel SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (SetProperty(ref _selectedModel, value))
                {
                    FilterTrims();
                }
            }
        }

        private TrimLevel _selectedTrim;
        public TrimLevel SelectedTrim
        {
            get => _selectedTrim;
            set => SetProperty(ref _selectedTrim, value);
        }

        public event Action? CloseRequested;
        public event Action<string>? ShowError;
        public event Action<string>? ShowSuccess;
        public event Action? VehicleAdded;

        public AddVehicleViewModel()
        {
            NewVehicle = new Vehicle { Status = "Dostępny", IsUsed = false };
        }

        public async Task LoadAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                var engines = await uow.Engines.GetAllAsync();
                var models = await uow.VehicleModels.GetAllAsync();
                var trims = await uow.TrimLevels.GetAllAsync();

                AvailableEngines.Clear();
                foreach (var e in engines) AvailableEngines.Add(e);

                AvailableModels.Clear();
                foreach (var m in models) AvailableModels.Add(m);

                _allTrims = trims.ToList();
                AvailableTrims.Clear();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke("Nie udało się pobrać danych słownikowych.");
            }
        }

        private void FilterTrims()
        {
            AvailableTrims.Clear();
            SelectedTrim = null; 

            if (SelectedModel != null)
            {
                var filteredTrims = _allTrims.Where(t => t.ModelID == SelectedModel.ModelID);

                foreach (var trim in filteredTrims)
                {
                    AvailableTrims.Add(trim);
                }
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(NewVehicle.VIN) || SelectedEngine == null || SelectedModel == null || SelectedTrim == null)
            {
                ShowError?.Invoke("Uzupełnij VIN oraz wybierz silnik, model i wersję wyposażenia.");
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                NewVehicle.EngineID = SelectedEngine.EngineID;
                NewVehicle.TrimID = SelectedTrim.TrimID;
                NewVehicle.DealershipID = 1;

                await uow.Vehicles.AddAsync(NewVehicle);
                await ctx.SaveChangesAsync();

                ShowSuccess?.Invoke("Pojazd został poprawnie dodany.");
                VehicleAdded?.Invoke();
                CloseRequested?.Invoke();
            }
            catch (Exception ex)
            {
                ShowError?.Invoke($"Błąd podczas zapisu do bazy: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke();
        }
    }
}