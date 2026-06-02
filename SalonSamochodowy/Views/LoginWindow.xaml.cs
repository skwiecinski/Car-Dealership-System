using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy.Views
{
    public partial class LoginWindow : FluentWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = new LoginViewModel();
            vm.LoginSucceeded += user =>
            {
                SessionContext.CurrentUser = user; // zeby miec info o userze, jest static, takze mozna sie odwolywac bez tworzenia klasy
                var main = new MainWindow(user);
                main.Show();
                Close();
            };
            vm.ExitRequested += () => Application.Current.Shutdown();

            DataContext = vm;
        }
    }
}
