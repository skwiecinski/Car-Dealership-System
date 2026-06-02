using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using SalonSamochodowy.Entities;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

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
    }
}
