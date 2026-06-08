using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using System.IO;

namespace SalonSamochodowy.Services
{
    public static class PdfChartGenerator
    {
        public static byte[] GeneratePieChart(List<(string Label, double Value)> data, int width = 600, int height = 400)
        {
            var series = new List<ISeries>();
            foreach (var item in data)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new double[] { item.Value },
                    Name = item.Label,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{item.Value}"
                });
            }

            var chart = new SKPieChart
            {
                Width = width,
                Height = height,
                Series = series,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
                Background = SKColors.White
            };

            using var image = chart.GetImage();
            using var dataImage = image.Encode(SKEncodedImageFormat.Png, 100);
            return dataImage.ToArray();
        }

        public static byte[] GenerateColumnChart(List<(string Label, double Value)> data, string yAxisTitle = "", int width = 800, int height = 400)
        {
            var values = new double[data.Count];
            var labels = new string[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                values[i] = data[i].Value;
                labels[i] = data[i].Label;
            }

            var chart = new SKCartesianChart
            {
                Width = width,
                Height = height,
                Series = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = values,
                        Name = yAxisTitle,
                        DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                    }
                },
                XAxes = new[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsRotation = 15,
                        TextSize = 14
                    }
                },
                YAxes = new[]
                {
                    new Axis
                    {
                        Name = yAxisTitle,
                        NameTextSize = 14,
                        TextSize = 14
                    }
                },
                Background = SKColors.White
            };

            using var image = chart.GetImage();
            using var dataImage = image.Encode(SKEncodedImageFormat.Png, 100);
            return dataImage.ToArray();
        }
    }
}
