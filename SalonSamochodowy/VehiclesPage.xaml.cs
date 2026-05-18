using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SalonSamochodowy
{
    public partial class VehiclesPage : Page
    {
        // Kolekcja, z którą połączony jest widok XAML
        public ObservableCollection<VehicleItem> VehicleList { get; set; }

        public VehiclesPage()
        {
            InitializeComponent();

            // Pusta kolekcja - backend wypelni z bazy
            VehicleList = new ObservableCollection<VehicleItem>();

            // Ustawienie kontekstu danych dla XAML
            DataContext = this;
        }
    }

    // Klasa pomocnicza reprezentująca jeden pojazd na liście
    public class VehicleItem
    {
        public string FullName { get; set; }
        public string EngineInfo { get; set; }
        public string VIN { get; set; }
        public string Price { get; set; }
        public string Status { get; set; }

        // Kolory dynamicznie przypisywane do statusu
        public string StatusTextColor { get; set; }
        public string StatusBackgroundColor { get; set; }
        public string StatusBorderColor { get; set; }
    }
}