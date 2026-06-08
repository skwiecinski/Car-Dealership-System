using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;
using PasswordBox = Wpf.Ui.Controls.PasswordBox;

namespace SalonSamochodowy.Views
{
    public partial class EditEmployeeWindow : FluentWindow
    {
        public EditEmployeeViewModel ViewModel { get; }

        public EditEmployeeWindow(AccountRow accountRow, System.Collections.Generic.IEnumerable<DealershipItem> dealerships, Window? owner = null)
        {
            InitializeComponent();

            ViewModel = new EditEmployeeViewModel();
            ViewModel.LoadData(accountRow, dealerships);
            DataContext = ViewModel;

            if (owner != null)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            ViewModel.SaveSucceeded += () =>
            {
                DialogResult = true;
                Close();
            };

            ViewModel.CancelRequested += () =>
            {
                DialogResult = false;
                Close();
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                ViewModel.NewPassword = pb.Password;
            }
        }
    }
}
