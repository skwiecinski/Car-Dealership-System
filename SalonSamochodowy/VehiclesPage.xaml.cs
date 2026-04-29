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

            // Generowanie testowych danych
            VehicleList = new ObservableCollection<VehicleItem>
            {
                new VehicleItem
                {
                    FullName = "BMW Seria 3 (G20) M-Sport",
                    EngineInfo = "Silnik: 2.0 Benzyna (245 KM)",
                    VIN = "WBA31BA00K1234567",
                    Price = "245 000 PLN",
                    Status = "Dostępny",
                    StatusTextColor = "#44C767", StatusBackgroundColor = "#112C1E", StatusBorderColor = "#2D9A4A"
                },
                new VehicleItem
                {
                    FullName = "Audi A6 Avant quattro",
                    EngineInfo = "Silnik: 3.0 TDI (286 KM)",
                    VIN = "WAUZZZ4A5LN098765",
                    Price = "320 500 PLN",
                    Status = "Zarezerwowany",
                    StatusTextColor = "#F0B82B", StatusBackgroundColor = "#332A12", StatusBorderColor = "#D3A125"
                },
                new VehicleItem
                {
                    FullName = "Mercedes-Benz GLC 220d",
                    EngineInfo = "Silnik: 2.0 Diesel (197 KM)",
                    VIN = "W1N2539151F112233",
                    Price = "289 900 PLN",
                    Status = "Dostępny",
                    StatusTextColor = "#44C767", StatusBackgroundColor = "#112C1E", StatusBorderColor = "#2D9A4A"
                },
                new VehicleItem
                {
                    FullName = "BMW X5 xDrive40i",
                    EngineInfo = "Silnik: 3.0 Benzyna (381 KM)",
                    VIN = "5UXCR6C04M9876543",
                    Price = "415 000 PLN",
                    Status = "Sprzedany",
                    StatusTextColor = "#ED6262", StatusBackgroundColor = "#3D1D1D", StatusBorderColor = "#D34545"
                }
            };

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