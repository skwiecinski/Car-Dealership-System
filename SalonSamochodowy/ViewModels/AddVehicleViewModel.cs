using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Repositories; // Zakładam, że tu macie UnitOfWork

namespace SalonSamochodowy.ViewModels
{
    public partial class AddVehicleViewModel : ObservableObject
    {
        public Vehicle NewVehicle { get; set; }

        // Inicjalizacja pustych kolekcji od razu, zgodnie z Waszym standardem
        public ObservableCollection<Engine> AvailableEngines { get; } = new();
        public ObservableCollection<TrimLevel> AvailableTrims { get; } = new();

        public Engine SelectedEngine { get; set; }

        public TrimFeature SelectedTrim { get; set; }

        public AddVehicleViewModel()
        {
            // Przygotowanie pustego obiektu przed wyświetleniem
            NewVehicle = new Vehicle
            {
                Status = "Dostępny",
                IsUsed = false
            };
        }

        // Metoda do wywołania np. w zdarzeniu Loaded okna AddVehicleWindow
        public async Task LoadFromDbAsync()
        {
            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                // Zakładam, że repozytoria mają taką metodę
                var engines = await uow.Engines.GetAllAsync();
                var trims = await uow.TrimLevels.GetAllAsync();

                AvailableEngines.Clear();
                foreach (var e in engines)
                {
                    AvailableEngines.Add(e);
                }

                AvailableTrims.Clear();
                foreach (var t in trims)
                {
                    AvailableTrims.Add(t);
                }
            }
            catch (Exception ex)
            {
                // Tutaj jak w Waszym kodzie - puste catch lub logowanie błędu
            }
        }

        [RelayCommand]
        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(NewVehicle.VIN) || SelectedEngine == null || SelectedTrim == null)
            {
                MessageBox.Show("Uzupełnij VIN oraz wybierz silnik i wersję wyposażenia.", "Brak danych", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var ctx = new AppDbContext();
                using var uow = new UnitOfWork(ctx);

                NewVehicle.EngineID = SelectedEngine.EngineID;
                NewVehicle.TrimID = SelectedTrim.TrimID;

                // Ustawiamy Dealership - na sztywno, lub z SessionContext
                NewVehicle.DealershipID = 1;

                // Dodawanie obiektu do repozytorium
                // UWAGA: użyj właściwej nazwy metody z Waszego interfejsu (np. AddAsync, Insert)
                await uow.Vehicles.AddAsync(NewVehicle);

                // Zapisanie zmian - zależy, czy robicie to przez UoW czy prosto z kontekstu
                await ctx.SaveChangesAsync();
                // lub await uow.CompleteAsync() - dopasuj do Waszej implementacji UoW

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas zapisu do bazy: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}