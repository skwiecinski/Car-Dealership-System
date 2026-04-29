using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SalonSamochodowy
{
    public partial class DashboardPage : Page
    {
        public ISeries[] SalesSeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public ISeries[] VehicleStructureSeries { get; set; }

        // Definicja stylu tekstu legendy
        public SolidColorPaint LegendTextStyle { get; set; } =
            new SolidColorPaint(new SKColor(138, 141, 152)); // Kolor #8A8D98

        public DashboardPage()
        {
            InitializeComponent();

            // Kolory HEX z projektu
            var accentColor = new SKColor(91, 89, 232);      // #5B59E8
            var axisTextColor = new SKColor(138, 141, 152);  // #8A8D98
            var separatorColor = new SKColor(45, 48, 56);    // #2D3038
            var chartBackground = new SKColor(34, 36, 44);   // #22242C

            // 1. WYKRES SŁUPKOWY
            SalesSeries = new ISeries[] {
                new ColumnSeries<int> {
                    Values = new int[] { 28, 22, 17, 14, 9 },
                    Fill = new SolidColorPaint(accentColor),
                    Name = "Sprzedaż",
                    MaxBarWidth = 35,
                    Rx = 6, Ry = 6
                }
            };

            XAxes = new Axis[] {
                new Axis {
                    Labels = new string[] { "Jan K.", "Anna N.", "Piotr Z.", "Marta W.", "Tomasz U." },
                    LabelsPaint = new SolidColorPaint(axisTextColor),
                }
            };

            YAxes = new Axis[] {
                new Axis {
                    LabelsPaint = new SolidColorPaint(axisTextColor),
                    SeparatorsPaint = new SolidColorPaint(separatorColor) { StrokeThickness = 1 }
                }
            };

            // 2. WYKRES KOŁOWY
            VehicleStructureSeries = new ISeries[] {
                new PieSeries<int> { Values = new int[] { 30 }, Name = "BMW", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(45, 127, 249)) },
                new PieSeries<int> { Values = new int[] { 25 }, Name = "Audi", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(138, 81, 212)) },
                new PieSeries<int> { Values = new int[] { 45 }, Name = "Inne", InnerRadius = 60, Fill = new SolidColorPaint(new SKColor(87, 89, 96)) }
            };

            DataContext = this;
        }
    }
}