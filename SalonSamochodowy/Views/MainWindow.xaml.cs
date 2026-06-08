using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SalonSamochodowy.Entities;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace SalonSamochodowy.Views
{
    public partial class MainWindow : FluentWindow
    {
        public AppUser LoggedInUser => _vm.LoggedInUser;

        private readonly MainWindowViewModel _vm;

        public MainWindow(AppUser loggedIn)
        {
            InitializeComponent();

            _vm = ((App)Application.Current).Services.GetRequiredService<MainWindowViewModel>();
            _vm.Initialize(loggedIn);
            DataContext = _vm;

            if (_vm.AccessDeniedMessage != null)
            {
                System.Windows.MessageBox.Show(
                    _vm.AccessDeniedMessage,
                    "Brak uprawnień",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }

            _vm.LogoutRequested += () =>
            {
                var login = new LoginWindow();
                login.Show();
                Close();
            };

            _vm.ShowProfileRequested += info =>
            {
                System.Windows.MessageBox.Show(
                    info,
                    "Mój profil",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            };

            if (_vm.StartupPageType != null)
            {
                RootNavigation.Loaded += (s, e) => RootNavigation.Navigate(_vm.StartupPageType);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = RootNavigation.SelectedItem as INavigationViewItem;
            string pageName = selectedItem?.Content?.ToString() ?? "Strona główna";

            // Zapytaj aktywną stronę, czy posiada samouczek
            var tourMessage = new SalonSamochodowy.Messages.StartTourRequestMessage(pageName);
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(tourMessage);

            if (tourMessage.HasReceivedResponse && tourMessage.Response == true)
            {
                // Strona sama obsłużyła samouczek - nie robimy nic więcej
                return;
            }
            
            string helpTitle = $"Pomoc: {pageName}";
            string helpText = "Wybierz zakładkę z menu po lewej stronie, aby rozpocząć pracę.";

            switch (pageName)
            {
                case "Moje Zamówienia":
                    helpText = "W tej sekcji możesz przeglądać listę swoich zamówień, ich statusy oraz pobrać raport, jeżeli zamówienie zostało przypisane do Twojego konta.";
                    break;
                case "Dashboard":
                    helpText = "Tablica podsumowująca najważniejsze statystyki w salonie. Znajdziesz tu szybki podgląd liczby aut, nowych zamówień i trwających napraw.";
                    break;
                case "Klienci i Sprzedaż":
                    helpText = "Moduł do zarządzania bazą klientów. Możesz tutaj zarejestrować nowego klienta, wygenerować dla niego raport oraz przejrzeć pełną historię zakupów.";
                    break;
                case "Trwające Sprzedaże":
                    helpText = "Lista aktualnie procedowanych zamówień. Umożliwia zmianę statusu i weryfikację postępu dla każdego zlecenia (np. z 'Gotowe do odbioru' na 'Zrealizowane').";
                    break;
                case "Dodaj zamówienie":
                    helpText = "Zacznij nową sprzedaż. Wybierz klienta (lub stwórz nowego), dopasuj odpowiedni pojazd, dobierz konfigurację opcjonalną i sfinalizuj transakcję.";
                    break;
                case "Zarządzanie Pojazdami":
                    helpText = "Zarządzaj autami na placu. Możesz tu dodawać nowe pojazdy z bazy fabrycznej (Silnik + Model), określać, czy są nowe, czy używane, i podawać numery VIN.";
                    break;
                case "Usługi Serwisowe":
                    helpText = "Warsztat mechaniczny. Serwisanci mogą tu przejmować nowe zlecenia (np. montaż akcesoriów), zmieniać ich statusy na robocze lub ostatecznie ukończone.";
                    break;
                case "Raporty":
                    helpText = "Generuj zaawansowane PDF-y, aby poznać wydajność firmy. Raporty posiadają pełną analitykę, kolorowe wykresy i rankingi Twoich pracowników.";
                    break;
                case "Administracja":
                    helpText = "Panel administratora do zarządzania bazą danych (np. dodawanie nowych salonów) i rejestrowania nowych kont pracowniczych z konkretnymi stanowiskami.";
                    break;
            }

            System.Windows.MessageBox.Show(
                helpText,
                helpTitle,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
