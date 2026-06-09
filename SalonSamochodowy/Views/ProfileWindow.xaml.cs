using System.Windows;
using Wpf.Ui.Controls;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Views
{
    public partial class ProfileWindow : FluentWindow
    {
        public ProfileWindow(AppUser user)
        {
            InitializeComponent();
            
            var roleKey = user.Role?.RoleName switch
            {
                "Administrator" => "Role_Admin",
                "Kierownik" => "Role_Manager",
                "Sprzedawca" => "Role_Salesman",
                "Serwisant" => "Role_Mechanic",
                "Klient" => "Role_Client",
                _ => null
            };

            string displayRole = roleKey != null ? SalonSamochodowy.Services.LocalizationHelper.GetString(roleKey) : (user.Role?.RoleName ?? "—");

            DataContext = new
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                DisplayRole = displayRole
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
