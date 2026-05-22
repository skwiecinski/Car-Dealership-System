using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class LoginWindow : FluentWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = new LoginViewModel();
            vm.LoginSucceeded += user =>
            {
                var main = new MainWindow(user);
                main.Show();
                Close();
            };
            vm.ExitRequested += () => Application.Current.Shutdown();

            DataContext = vm;
        }
    }
}
