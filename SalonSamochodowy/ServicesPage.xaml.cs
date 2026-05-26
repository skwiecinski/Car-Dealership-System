using System.Windows;
using System.Windows.Controls;
using SalonSamochodowy.ViewModels;

namespace SalonSamochodowy
{
    public partial class ServicesPage : Page
    {
        public ServicesPage()
        {
            InitializeComponent();
            DataContext = new ServicesPageViewModel();

            Loaded += async (s, e) => await ((ServicesPageViewModel)DataContext).LoadFromDbAsync();
        }
    }
}

