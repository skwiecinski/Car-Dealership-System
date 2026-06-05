using System.Windows;
using SalonSamochodowy.ViewModels;
using Wpf.Ui.Controls;

namespace SalonSamochodowy
{
    public partial class EditClientWindow : FluentWindow
    {
        private readonly EditClientViewModel _vm;

        public EditClientWindow(ClientModel client, Window? owner = null)
        {
            InitializeComponent();

            _vm = new EditClientViewModel(client);
            DataContext = _vm;

            if (owner != null)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            _vm.SaveSucceeded += () =>
            {
                DialogResult = true;
                Close();
            };

            _vm.CancelRequested += () =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}