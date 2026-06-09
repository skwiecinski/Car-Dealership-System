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
            Type pageType = selectedItem?.TargetPageType;
            string pageTypeIdentifier = pageType?.Name ?? "Unknown";

            // Zapytaj aktywną stronę, czy posiada samouczek
            var tourMessage = new SalonSamochodowy.Messages.StartTourRequestMessage(pageTypeIdentifier);
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(tourMessage);

            if (tourMessage.HasReceivedResponse && tourMessage.Response == true)
            {
                // Strona sama obsłużyła samouczek - nie robimy nic więcej
                return;
            }
            
            string helpTitleKey = "Help_Title";
            string helpTitleStr = SalonSamochodowy.Services.LocalizationHelper.GetString(helpTitleKey);
            
            string helpTitle = $"{helpTitleStr}: {selectedItem?.Content?.ToString() ?? ""}";
            
            string helpTextKey = $"Help_{pageTypeIdentifier}";
            string helpText = SalonSamochodowy.Services.LocalizationHelper.GetString(helpTextKey);

            if (helpText == helpTextKey) // If missing
            {
                helpText = SalonSamochodowy.Services.LocalizationHelper.GetString("Help_Default");
            }

            System.Windows.MessageBox.Show(
                helpText,
                helpTitle,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        private void LangPl_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("pl");
        }

        private void LangEn_Click(object sender, RoutedEventArgs e)
        {
            SwitchLanguage("en");
        }

        private void SwitchLanguage(string lang)
        {
            var dict = new ResourceDictionary();
            dict.Source = new System.Uri($"pack://application:,,,/Resources/Languages/Strings.{lang}.xaml");

            var merged = Application.Current.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                if (merged[i].Source != null && merged[i].Source.OriginalString.Contains("Strings."))
                {
                    merged.RemoveAt(i);
                }
            }

            merged.Add(dict);

            if (lang == "pl")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
                LangPlBtn.Opacity = 1.0;
                LangEnBtn.Opacity = 0.4;
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                LangPlBtn.Opacity = 0.4;
                LangEnBtn.Opacity = 1.0;
            }

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new SalonSamochodowy.Messages.LanguageChangedMessage());
        }
    }
}
