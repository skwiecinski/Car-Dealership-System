using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SalonSamochodowy.Entities;

namespace SalonSamochodowy.Services
{
    public class ReportGeneratorService
    {
        public ReportGeneratorService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private void ComposeHeader(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(title).FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(100).AlignRight().Text("LeAuto Salon").FontSize(16).SemiBold().FontColor(Colors.Grey.Darken2);
            });
        }

        private void DrawBarChart(IContainer container, string title, List<(string Label, float Value)> data, string valueFormat = "{0}")
        {
            if (!data.Any()) return;
            
            var maxValue = data.Max(x => x.Value);
            if (maxValue == 0) maxValue = 1;

            string[] colors = { Colors.Blue.Medium, Colors.Green.Medium, Colors.Orange.Medium, Colors.Purple.Medium, Colors.Teal.Medium, Colors.Red.Medium };

            container.PaddingBottom(15).Column(column =>
            {
                column.Item().PaddingBottom(15).Text(title).FontSize(14).SemiBold();
                
                int colorIndex = 0;
                foreach (var item in data)
                {
                    column.Item().PaddingBottom(8).Row(row =>
                    {
                        row.ConstantItem(150).AlignMiddle().Text(item.Label).FontSize(10);
                        
                        row.RelativeItem().AlignMiddle().Column(barColumn => 
                        {
                            barColumn.Item().Height(16).Background(Colors.Grey.Lighten3).Row(r =>
                            {
                                float percentage = item.Value / maxValue;
                                if (percentage > 0) r.RelativeItem(percentage).Background(colors[colorIndex % colors.Length]);
                                if (percentage < 1) r.RelativeItem(1 - percentage);
                            });
                        });

                        row.ConstantItem(80).AlignMiddle().AlignRight().Text(string.Format(valueFormat, item.Value)).FontSize(10).SemiBold();
                    });
                    colorIndex++;
                }
            });
        }

        public async Task<string> GenerateOrdersByStatusReportAsync(IEnumerable<SalesOrder> orders)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Raport_Zamowien_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            var groupedOrders = orders.GroupBy(o => o.Status).OrderBy(g => g.Key).ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Raport zamówień wg statusów"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        var statusCounts = orders.GroupBy(o => o.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
                        
                        var pieChartData = statusCounts.Select(x => (x.Status, (double)x.Count)).ToList();
                        if (pieChartData.Any())
                        {
                            var chartImage = PdfChartGenerator.GeneratePieChart(pieChartData, 500, 300);
                            column.Item().PaddingBottom(20).Image(chartImage);
                        }

                        foreach (var group in groupedOrders)
                        {
                            column.Item().PaddingBottom(5).Text($"Status: {group.Key} ({group.Count()} zamówień)").FontSize(16).SemiBold().FontColor(Colors.Black);
                            
                            column.Item().PaddingBottom(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50); 
                                    columns.RelativeColumn();   
                                    columns.RelativeColumn();   
                                    columns.ConstantColumn(80); 
                                    columns.ConstantColumn(80); 
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("ID").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Pojazd").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Klient").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Data").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Kwota").SemiBold();
                                });

                                foreach (var order in group)
                                {
                                    table.Cell().PaddingVertical(2).Text($"#{order.OrderID}");
                                    table.Cell().PaddingVertical(2).Text($"{order.Vehicle?.Trim?.Model?.Brand} {order.Vehicle?.Trim?.Model?.ModelName}");
                                    table.Cell().PaddingVertical(2).Text($"{order.Client?.User?.FirstName} {order.Client?.User?.LastName}");
                                    table.Cell().PaddingVertical(2).Text($"{order.OrderDate:dd.MM.yyyy}");
                                    table.Cell().PaddingVertical(2).AlignRight().Text($"{order.FinalPrice:C2}");
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Strona ");
                        x.CurrentPageNumber();
                        x.Span(" z ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        public async Task<string> GenerateClientReportAsync(Client client, IEnumerable<SalesOrder> orders)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Raport_Klienta_{client.ClientID}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, $"Raport Klienta: {client.User?.FirstName} {client.User?.LastName}"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Item().PaddingBottom(10).Text("Podsumowanie klienta").FontSize(14).SemiBold();
                        column.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().Text($"E-mail: {client.User?.Email}");
                            inner.Item().Text($"Telefon: {client.Phone}");
                            
                            var finishedOrders = orders.Where(o => o.Status == OrderStatuses.Finished || o.Status == OrderStatuses.FinishedAlt).ToList();
                            decimal totalSpent = finishedOrders.Sum(o => o.FinalPrice);
                            decimal avgSpent = finishedOrders.Any() ? finishedOrders.Average(o => o.FinalPrice) : 0;
                            
                            inner.Item().PaddingTop(5).Text($"Liczba zrealizowanych zamówień: {finishedOrders.Count}");
                            inner.Item().Text($"Całkowita kwota zakupów: {totalSpent:C2}").SemiBold();
                            inner.Item().Text($"Średnia wartość koszyka: {avgSpent:C2}");
                        });
                        column.Item().PaddingBottom(20);

                        column.Item().PaddingBottom(10).Text("Historia Zakupów").FontSize(14).SemiBold();
                        
                        if (!orders.Any())
                        {
                            column.Item().Text("Brak zarejestrowanych zamówień.").Italic();
                        }
                        else
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("ID").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Pojazd").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Data Zam.").SemiBold();
                                    header.Cell().BorderBottom(1).PaddingBottom(5).AlignRight().Text("Kwota").SemiBold();
                                });

                                foreach (var order in orders.OrderByDescending(o => o.OrderDate))
                                {
                                    table.Cell().PaddingVertical(2).Text($"#{order.OrderID}");
                                    table.Cell().PaddingVertical(2).Text($"{order.Vehicle?.Trim?.Model?.Brand} {order.Vehicle?.Trim?.Model?.ModelName}");
                                    table.Cell().PaddingVertical(2).Text(order.Status);
                                    table.Cell().PaddingVertical(2).Text($"{order.OrderDate:dd.MM.yyyy}");
                                    table.Cell().PaddingVertical(2).AlignRight().Text($"{order.FinalPrice:C2}");
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        public async Task<string> GenerateSalesRankingsReportAsync(IEnumerable<SalesOrder> orders, IEnumerable<Worker> workers, IEnumerable<AppUser> users, IEnumerable<Dealership> dealerships)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Ranking_Sprzedazy_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            var finalizedOrders = orders.Where(o => o.Status == OrderStatuses.Finished || o.Status == OrderStatuses.FinishedAlt).ToList();

            var dealershipSales = dealerships.Select(d => new
            {
                Dealership = d,
                TotalSales = finalizedOrders.Where(o => workers.FirstOrDefault(w => w.UserID == o.WorkerID)?.DealershipID == d.DealershipID).Sum(o => o.FinalPrice)
            }).OrderByDescending(x => x.TotalSales).ToList();

            var workerSales = workers.Select(w => new
            {
                User = users.FirstOrDefault(u => u.UserID == w.UserID),
                TotalSales = finalizedOrders.Where(o => o.WorkerID == w.UserID).Sum(o => o.FinalPrice)
            }).Where(x => x.User != null && x.TotalSales > 0).OrderByDescending(x => x.TotalSales).Take(10).ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Rankingi Sprzedaży"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        decimal allTimeRevenue = finalizedOrders.Sum(o => o.FinalPrice);
                        column.Item().PaddingBottom(20).Background(Colors.Grey.Lighten4).Padding(10).Text($"Łączny wygenerowany przychód firmy: {allTimeRevenue:C2}").FontSize(14).SemiBold();

                        var dChartData = dealershipSales.Select(x => ($"{x.Dealership.Name} ({x.Dealership.City})", (float)x.TotalSales)).ToList();
                        column.Item().Element(c => DrawBarChart(c, "Ranking Salonów (wg przychodu)", dChartData, "{0:C2}"));

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        var wChartData = workerSales.Select(x => ($"{x.User?.FirstName} {x.User?.LastName}", (float)x.TotalSales)).ToList();
                        column.Item().Element(c => DrawBarChart(c, "Top 10 Sprzedawców (wg przychodu)", wChartData, "{0:C2}"));
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        public async Task<string> GenerateFullDatabaseDumpReportAsync(
            IEnumerable<Dealership> dealerships, 
            IEnumerable<Worker> workers, 
            IEnumerable<AppUser> users, 
            IEnumerable<SalesOrder> orders, 
            IEnumerable<Client> clients, 
            IEnumerable<Vehicle> vehicles,
            IEnumerable<Engine> engines,
            IEnumerable<TrimLevel> trimLevels,
            IEnumerable<Feature> features,
            IEnumerable<VehicleModel> models,
            IEnumerable<Job> jobs)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Pelny_Raport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Kompleksowy Raport Bazy Danych"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Item().PaddingBottom(20).Background(Colors.Grey.Lighten4).Padding(10).Column(inner =>
                        {
                            inner.Item().PaddingBottom(5).Text("Podsumowanie statystyczne").FontSize(14).SemiBold();
                            inner.Item().Text($"Zarejestrowane Salony: {dealerships.Count()}");
                            inner.Item().Text($"Dostępne Pojazdy: {vehicles.Count(v => v.Status != "Sprzedany")}");
                            inner.Item().Text($"Sprzedane Pojazdy: {vehicles.Count(v => v.Status == "Sprzedany")}");
                            inner.Item().Text($"Klienci w bazie: {clients.Count()}");
                            inner.Item().Text($"Zatrudnieni Pracownicy: {workers.Count()}");
                            inner.Item().Text($"Przetworzone Zamówienia: {orders.Count()}");
                        });

                        // Salony
                        column.Item().PaddingBottom(10).Text($"Salony").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Nazwa").SemiBold(); h.Cell().Text("Adres").SemiBold(); h.Cell().Text("Właściciel").SemiBold(); });
                            foreach(var d in dealerships)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(d.DealershipID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(d.Name);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{d.City}, {d.Address}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(d.Owner);
                            }
                        });

                        var availableVehicles = vehicles.Where(v => v.Status != "Sprzedany").ToList();
                        var soldVehicles = vehicles.Where(v => v.Status == "Sprzedany").ToList();

                        // Pojazdy Dostępne
                        column.Item().PaddingBottom(10).Text($"Dostępne Pojazdy ({availableVehicles.Count})").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.ConstantColumn(60); c.RelativeColumn(); c.ConstantColumn(80); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Model").SemiBold(); h.Cell().Text("Stan").SemiBold(); h.Cell().Text("VIN").SemiBold(); h.Cell().AlignRight().Text("Cena bazowa").SemiBold(); });
                            foreach(var v in availableVehicles)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.VehicleID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{v.Trim?.Model?.Brand} {v.Trim?.Model?.ModelName}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.IsUsed ? "Używany" : "Nowy");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.VIN);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{v.Trim?.BasePrice:C2}");
                            }
                        });

                        // Pojazdy Sprzedane
                        if (soldVehicles.Any())
                        {
                            column.Item().PaddingBottom(10).Text($"Sprzedane Pojazdy ({soldVehicles.Count})").FontSize(16).SemiBold();
                            column.Item().PaddingBottom(20).Table(t =>
                            {
                                t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.ConstantColumn(60); c.RelativeColumn(); c.ConstantColumn(80); });
                                t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Model").SemiBold(); h.Cell().Text("Stan").SemiBold(); h.Cell().Text("VIN").SemiBold(); h.Cell().AlignRight().Text("Cena bazowa").SemiBold(); });
                                foreach(var v in soldVehicles)
                                {
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.VehicleID.ToString());
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{v.Trim?.Model?.Brand} {v.Trim?.Model?.ModelName}");
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.IsUsed ? "Używany" : "Nowy");
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(v.VIN);
                                    t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{v.Trim?.BasePrice:C2}");
                                }
                            });
                        }

                        // Klienci
                        column.Item().PaddingBottom(10).Text($"Klienci").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Imię i Nazwisko").SemiBold(); h.Cell().Text("E-mail").SemiBold(); h.Cell().Text("Telefon").SemiBold(); });
                            foreach(var c in clients)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(c.ClientID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{c.User?.FirstName} {c.User?.LastName}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(c.User?.Email ?? "");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(c.Phone);
                            }
                        });

                        // Pracownicy
                        column.Item().PaddingBottom(10).Text($"Pracownicy").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Użytkownik").SemiBold(); h.Cell().Text("Salon").SemiBold(); h.Cell().AlignRight().Text("Wypłata").SemiBold(); });
                            foreach(var w in workers)
                            {
                                var user = users.FirstOrDefault(u => u.UserID == w.UserID);
                                var dealership = dealerships.FirstOrDefault(d => d.DealershipID == w.DealershipID);

                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(w.WorkerID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(user != null ? $"{user.FirstName} {user.LastName}" : "Nieznany");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(dealership?.Name ?? "Brak");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{w.Payroll:C2}");
                            }
                        });

                        // Modele
                        column.Item().PaddingBottom(10).Text($"Modele Samochodów").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Marka").SemiBold(); h.Cell().Text("Nazwa modelu").SemiBold(); });
                            foreach(var m in models)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(m.ModelID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(m.Brand);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(m.ModelName);
                            }
                        });

                        // Silniki
                        column.Item().PaddingBottom(10).Text($"Silniki").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.ConstantColumn(60); c.ConstantColumn(80); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Nazwa").SemiBold(); h.Cell().Text("Pojemność").SemiBold(); h.Cell().AlignRight().Text("Moc (KM)").SemiBold(); h.Cell().AlignRight().Text("Cena").SemiBold(); });
                            foreach(var e in engines)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(e.EngineID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{e.Brand} {e.EngineName}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(e.EngineSize);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text(e.Power.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{e.Price:C2}");
                            }
                        });

                        // Pakiety wyposazenia
                        column.Item().PaddingBottom(10).Text($"Warianty Wyposażenia").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.ConstantColumn(100); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Model").SemiBold(); h.Cell().Text("Wariant").SemiBold(); h.Cell().AlignRight().Text("Cena bazowa").SemiBold(); });
                            foreach(var tl in trimLevels)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(tl.TrimID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"{tl.Model?.Brand} {tl.Model?.ModelName}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(tl.TrimName);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{tl.BasePrice:C2}");
                            }
                        });

                        // Cechy/Opcje
                        column.Item().PaddingBottom(10).Text($"Cechy / Usługi Dodatkowe").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Nazwa").SemiBold(); h.Cell().Text("Kategoria").SemiBold(); });
                            foreach(var f in features)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(f.FeatureID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(f.FeatureName);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(f.Category);
                            }
                        });

                        // Zamówienia
                        column.Item().PaddingBottom(10).Text($"Wszystkie Zamówienia ({orders.Count()})").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.ConstantColumn(80); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Data").SemiBold(); h.Cell().Text("Status").SemiBold(); h.Cell().Text("Klient (ID)").SemiBold(); h.Cell().AlignRight().Text("Kwota").SemiBold(); });
                            foreach(var o in orders.OrderByDescending(x => x.OrderDate))
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(o.OrderID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(o.OrderDate.ToString("dd.MM.yyyy"));
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(o.Status);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"ID: {o.ClientID}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text($"{o.FinalPrice:C2}");
                            }
                        });

                        // Zlecenia Serwisowe
                        column.Item().PaddingBottom(10).Text($"Zlecenia Serwisowe ({jobs.Count()})").FontSize(16).SemiBold();
                        column.Item().PaddingBottom(20).Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                            t.Header(h => { h.Cell().Text("ID").SemiBold(); h.Cell().Text("Pojazd (ID)").SemiBold(); h.Cell().Text("Serwisant").SemiBold(); h.Cell().Text("Usługa").SemiBold(); h.Cell().Text("Status").SemiBold(); });
                            foreach(var j in jobs.OrderByDescending(x => x.CreatedAt))
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(j.JobID.ToString());
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text($"Auto ID: {j.VehicleID}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(j.Worker?.User != null ? $"{j.Worker.User.FirstName} {j.Worker.User.LastName}" : $"Worker ID: {j.WorkerID}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(j.Feature?.FeatureName ?? $"Feature ID: {j.FeatureID}");
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(j.Status);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        // --- NEW REPORTS --- //

        public async Task<string> GenerateMonthlyRevenueReportAsync(IEnumerable<SalesOrder> orders)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Przychody_Miesieczne_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            var finalizedOrders = orders.Where(o => o.Status == OrderStatuses.Finished || o.Status == OrderStatuses.FinishedAlt).ToList();

            var monthlySales = finalizedOrders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new { 
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Label = $"{g.Key.Month:D2}/{g.Key.Year}", 
                    Revenue = g.Sum(o => o.FinalPrice),
                    YearMonth = g.Key.Year * 100 + g.Key.Month 
                })
                .OrderBy(x => x.YearMonth)
                .ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Raport Przychodów Miesięcznych"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        var chartData = monthlySales.Select(x => ($"{x.Month:00}/{x.Year}", (double)x.Revenue)).ToList();
                        var chartImage = PdfChartGenerator.GenerateColumnChart(chartData, "Przychód (PLN)", 800, 400);
                        column.Item().Image(chartImage);

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        if (!monthlySales.Any())
                        {
                            column.Item().Text("Brak danych finansowych do wyświetlenia.").Italic();
                        }
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        public async Task<string> GenerateModelPopularityReportAsync(IEnumerable<SalesOrder> orders)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Popularnosc_Modeli_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            
            var popularModels = orders
                .Where(o => o.Vehicle != null && o.Vehicle.Trim != null && o.Vehicle.Trim.Model != null)
                .GroupBy(o => new { o.Vehicle.Trim.Model.Brand, o.Vehicle.Trim.Model.ModelName })
                .Select(g => new { Brand = g.Key.Brand, Model = g.Key.ModelName, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Trendy: Popularność Modeli"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        var chartData = popularModels.Select(x => ($"{x.Brand} {x.Model}", (double)x.Count)).ToList();
                        var chartImage = PdfChartGenerator.GeneratePieChart(chartData, 600, 400);
                        column.Item().PaddingBottom(10).Text("Udział w sprzedaży (Top 10)").FontSize(14).SemiBold();
                        column.Item().Image(chartImage);
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }

        public async Task<string> GenerateServiceEfficiencyReportAsync(IEnumerable<Job> jobs)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"Wydajnosc_Serwisu_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            
            var completedJobs = jobs.Where(j => j.Status == JobStatuses.Finished).ToList();

            var workerJobs = completedJobs
                .GroupBy(j => j.Worker)
                .Select(g => new { 
                    WorkerName = g.Key.User != null ? $"{g.Key.User.FirstName} {g.Key.User.LastName}" : "Nieznany", 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var popularFeatures = completedJobs
                .GroupBy(j => j.Feature)
                .Select(g => new { FeatureName = g.Key != null ? g.Key.FeatureName : "Nieznana usługa", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(c => ComposeHeader(c, "Raport Wydajności Serwisu"));

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        if (!workerJobs.Any() && !popularFeatures.Any())
                        {
                            column.Item().Text("Brak ukończonych zleceń serwisowych do wyświetlenia w statystykach.").Italic();
                            return;
                        }

                        var wChartData = workerJobs.Select(x => (x.WorkerName, (double)x.Count)).ToList();
                        var wChartImage = PdfChartGenerator.GenerateColumnChart(wChartData, "Liczba napraw", 800, 350);
                        column.Item().PaddingBottom(10).Text("Top 10 Serwisantów (wg liczby zleceń)").FontSize(14).SemiBold();
                        column.Item().Image(wChartImage);

                        column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        var fChartData = popularFeatures.Select(x => (x.FeatureName, (double)x.Count)).ToList();
                        var fChartImage = PdfChartGenerator.GeneratePieChart(fChartData, 600, 350);
                        column.Item().PaddingBottom(10).Text("Top 5 najczęściej instalowanych części/usług").FontSize(14).SemiBold();
                        column.Item().Image(fChartImage);
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Strona "); x.CurrentPageNumber(); x.Span(" z "); x.TotalPages(); });
                });
            }).GeneratePdf(filePath);

            return await Task.FromResult(filePath);
        }
    }
}
