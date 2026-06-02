using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy.Views
{
    public partial class AdminPage : Page
    {
        private readonly AdminPageViewModel _vm = new AdminPageViewModel();

        public AdminPage()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.ShowMessage += (msg, img) => System.Windows.MessageBox.Show(
                msg, "Administracja",
                System.Windows.MessageBoxButton.OK,
                img);

            _vm.ConfirmDelete += msg =>
            {
                var res = System.Windows.MessageBox.Show(
                    msg, "Usuń pracownika",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);
                return res == MessageBoxResult.Yes;
            };

            _vm.GetPassword += () => TxtPassword.Password;
            _vm.ClearPassword += () => TxtPassword.Password = "";

            _vm.GetLoggedInUserId += () =>
            {
                var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                return main?.LoggedInUser?.UserID ?? -1;
            };

            Loaded += async (s, e) => await _vm.LoadAllAsync();
        }
    }
}
