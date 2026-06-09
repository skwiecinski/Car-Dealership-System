using System;
using System.Collections.Generic;
using System.Linq;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;

namespace SalonSamochodowy
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            Random rnd = new Random(1337); // Stałe ziarno dla powtarzalności generowanych danych

            // 1. Roles
            if (!context.AppRoles.Any())
            {
                context.AppRoles.AddRange(
                    new AppRole { RoleName = RoleNames.Admin, PermissionLevel = 0 },
                    new AppRole { RoleName = RoleNames.Kierownik, PermissionLevel = 1 },
                    new AppRole { RoleName = RoleNames.Sprzedawca, PermissionLevel = 2 },
                    new AppRole { RoleName = RoleNames.Serwisant, PermissionLevel = 3 },
                    new AppRole { RoleName = RoleNames.Klient, PermissionLevel = 4 }
                );
                context.SaveChanges();
            }

            var rAdmin = context.AppRoles.First(r => r.RoleName == RoleNames.Admin);
            var rKierownik = context.AppRoles.First(r => r.RoleName == RoleNames.Kierownik);
            var rSprzedawca = context.AppRoles.First(r => r.RoleName == RoleNames.Sprzedawca);
            var rSerwisant = context.AppRoles.First(r => r.RoleName == RoleNames.Serwisant);
            var rKlient = context.AppRoles.First(r => r.RoleName == RoleNames.Klient);

            // 2. Base Admin user
            if (!context.AppUsers.Any(u => u.RoleID == rAdmin.RoleID))
            {
                context.AppUsers.Add(new AppUser { FirstName = "Anna", LastName = "Adminowa", Email = "admin@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rAdmin.RoleID, BirthDate = new DateTime(1980, 1, 15) });
                context.SaveChanges();
            }

            // 3. Dealerships
            if (!context.Dealerships.Any())
            {
                context.Dealerships.AddRange(
                    new Dealership { Name = "Salon Gliwice (Centrala)", Address = "ul. Akademicka 16", City = "Gliwice", Owner = "Zarząd" },
                    new Dealership { Name = "Salon Katowice Premium", Address = "ul. Chorzowska 50", City = "Katowice", Owner = "Zarząd" },
                    new Dealership { Name = "Salon Kraków", Address = "ul. Jasnogórska 2", City = "Kraków", Owner = "Zarząd" }
                );
                context.SaveChanges();
            }

            var dealerships = context.Dealerships.ToList();

            // 4. Base dictionary data
            var firstNames = new[] { "Piotr", "Michał", "Anna", "Katarzyna", "Tomasz", "Jan", "Kamil", "Marek", "Ewa", "Karolina", "Adam", "Marcin", "Mateusz", "Agnieszka", "Magdalena", "Jakub", "Maciej", "Paweł", "Monika", "Julia", "Zofia", "Hanna", "Krzysztof", "Szymon", "Bartosz" };
            var lastNames = new[] { "Kowalski", "Nowak", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński", "Lewandowski", "Zieliński", "Szymański", "Woźniak", "Dąbrowski", "Kozłowski", "Jankowski", "Mazur", "Wojciechowski", "Kwiatkowski", "Krawczyk", "Kaczmarek", "Piotrowski", "Grabowski" };

            // 5. Generate Workers (Managers, Sellers, Mechanics)
            if (!context.Workers.Any())
            {
                var workersToInsert = new List<Worker>();
                var usersToInsert = new List<AppUser>();

                // Helper to create worker
                Worker CreateWorker(string fname, string lname, string email, AppRole role, Dealership ds, decimal payroll)
                {
                    var user = new AppUser { FirstName = fname, LastName = lname, Email = email, PasswordHash = AuthService.HashPassword("123"), RoleID = role.RoleID, BirthDate = new DateTime(rnd.Next(1970, 2000), rnd.Next(1, 13), rnd.Next(1, 28)) };
                    usersToInsert.Add(user);
                    return new Worker { User = user, DealershipID = ds.DealershipID, Payroll = payroll, EndOfContractDate = new DateTime(2028, 12, 31) };
                }

                int sellerId = 1;
                int mechId = 1;

                foreach (var ds in dealerships)
                {
                    // 1 Manager per dealership
                    var manFn = firstNames[rnd.Next(firstNames.Length)];
                    var manLn = lastNames[rnd.Next(lastNames.Length)];
                    workersToInsert.Add(CreateWorker(manFn, manLn, $"kierownik.{ds.City.ToLower()}@salon.pl", rKierownik, ds, 10000m));

                    // 5-8 Sellers per dealership
                    int numSellers = rnd.Next(5, 9);
                    for (int i = 0; i < numSellers; i++)
                    {
                        var fn = firstNames[rnd.Next(firstNames.Length)];
                        var ln = lastNames[rnd.Next(lastNames.Length)];
                        workersToInsert.Add(CreateWorker(fn, ln, $"sprzedawca{sellerId++}@salon.pl", rSprzedawca, ds, rnd.Next(5000, 8000)));
                    }

                    // 5-8 Mechanics per dealership
                    int numMechs = rnd.Next(5, 9);
                    for (int i = 0; i < numMechs; i++)
                    {
                        var fn = firstNames[rnd.Next(firstNames.Length)];
                        var ln = lastNames[rnd.Next(lastNames.Length)];
                        workersToInsert.Add(CreateWorker(fn, ln, $"serwis{mechId++}@salon.pl", rSerwisant, ds, rnd.Next(5500, 7500)));
                    }
                }

                // Add test accounts explicitly for easy testing
                workersToInsert.Add(CreateWorker("Wiesław", "Testowy", "kierownik@salon.pl", rKierownik, dealerships[0], 12000m));
                workersToInsert.Add(CreateWorker("Tomasz", "Testowy", "sprzedawca@salon.pl", rSprzedawca, dealerships[0], 7000m));
                workersToInsert.Add(CreateWorker("Piotr", "Testowy", "serwis@salon.pl", rSerwisant, dealerships[0], 6500m));

                context.AppUsers.AddRange(usersToInsert);
                context.Workers.AddRange(workersToInsert);
                context.SaveChanges();
            }

            // 6. Generate Clients
            if (!context.Clients.Any())
            {
                var clientsToInsert = new List<Client>();
                var usersToInsert = new List<AppUser>();

                // Explicit test client
                var testUser = new AppUser { FirstName = "Jan", LastName = "Testowy", Email = "klient@wp.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(1990, 5, 5) };
                usersToInsert.Add(testUser);
                clientsToInsert.Add(new Client { User = testUser, NIP = "1234567890", Phone = "111-222-333" });

                for (int i = 0; i < 80; i++)
                {
                    var fn = firstNames[rnd.Next(firstNames.Length)];
                    var ln = lastNames[rnd.Next(lastNames.Length)];
                    var user = new AppUser { FirstName = fn, LastName = ln, Email = $"klient{i}@example.com", PasswordHash = AuthService.HashPassword("123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(rnd.Next(1960, 2002), rnd.Next(1, 13), rnd.Next(1, 28)) };
                    usersToInsert.Add(user);
                    clientsToInsert.Add(new Client { User = user, NIP = rnd.NextDouble() > 0.7 ? rnd.Next(1000000000, int.MaxValue).ToString() : "", Phone = $"{rnd.Next(100, 999)}-{rnd.Next(100, 999)}-{rnd.Next(100, 999)}" });
                }

                context.AppUsers.AddRange(usersToInsert);
                context.Clients.AddRange(clientsToInsert);
                context.SaveChanges();
            }

            // 7. Base vehicle catalogs (Models, Engines, Features, Trims)
            if (!context.VehicleModels.Any())
            {
                context.VehicleModels.AddRange(
                    new VehicleModel { Brand = "BMW", ModelName = "Seria 1" },
                    new VehicleModel { Brand = "BMW", ModelName = "Seria 3" },
                    new VehicleModel { Brand = "BMW", ModelName = "Seria 5" },
                    new VehicleModel { Brand = "BMW", ModelName = "X1" },
                    new VehicleModel { Brand = "BMW", ModelName = "X3" },
                    new VehicleModel { Brand = "BMW", ModelName = "X5" },
                    new VehicleModel { Brand = "Mini", ModelName = "Cooper" },
                    new VehicleModel { Brand = "Mini", ModelName = "Countryman" }
                );
                context.SaveChanges();
            }

            if (!context.Engines.Any())
            {
                context.Engines.AddRange(
                    new Engine { Brand = "BMW", EngineName = "1.5 TwinPower Turbo", EngineSize = "1.5L", Power = 140, Price = 0m },
                    new Engine { Brand = "BMW", EngineName = "2.0 TwinPower Turbo Benzyna", EngineSize = "2.0L", Power = 184, Price = 8000m },
                    new Engine { Brand = "BMW", EngineName = "2.0 TwinPower Turbo Diesel", EngineSize = "2.0L", Power = 190, Price = 10000m },
                    new Engine { Brand = "BMW", EngineName = "3.0 TwinPower Turbo Diesel", EngineSize = "3.0L", Power = 286, Price = 22000m },
                    new Engine { Brand = "BMW", EngineName = "3.0 TwinPower Turbo Benzyna", EngineSize = "3.0L", Power = 333, Price = 24000m },
                    new Engine { Brand = "BMW", EngineName = "M xDrive 4.4 V8", EngineSize = "4.4L", Power = 530, Price = 60000m },
                    new Engine { Brand = "Mini", EngineName = "1.5 Cooper", EngineSize = "1.5L", Power = 136, Price = 0m },
                    new Engine { Brand = "Mini", EngineName = "2.0 Cooper S", EngineSize = "2.0L", Power = 178, Price = 9000m },
                    new Engine { Brand = "Mini", EngineName = "2.0 John Cooper Works", EngineSize = "2.0L", Power = 231, Price = 18000m }
                );
                context.SaveChanges();
            }

            if (!context.Features.Any())
            {
                context.Features.AddRange(
                    new Feature { FeatureName = "Lakier metalik", Category = "Wygląd", Price = 3000m },
                    new Feature { FeatureName = "Czujniki parkowania", Category = "Akcesoria", Price = 5000m },
                    new Feature { FeatureName = "Kamera cofania", Category = "Akcesoria", Price = 2000m },
                    new Feature { FeatureName = "Alarm", Category = "Bezpieczeństwo", Price = 1500m },
                    new Feature { FeatureName = "Klimatyzacja 2-strefowa", Category = "Komfort", Price = 2500m },
                    new Feature { FeatureName = "Skórzane fotele", Category = "Wnętrze", Price = 6000m },
                    new Feature { FeatureName = "Nawigacja", Category = "Multimedia", Price = 4000m },
                    new Feature { FeatureName = "Pakiet sportowy M", Category = "Wygląd", Price = 12000m },
                    new Feature { FeatureName = "Alpejska Biel (bazowy)", Category = "Kolor", Price = 0m },
                    new Feature { FeatureName = "Czarny Szafir metalik", Category = "Kolor", Price = 3500m },
                    new Feature { FeatureName = "Szary Melbourne metalik", Category = "Kolor", Price = 3500m },
                    new Feature { FeatureName = "Niebieski Phytonic metalik", Category = "Kolor", Price = 4000m }
                );
                context.SaveChanges();
            }

            if (!context.TrimLevels.Any())
            {
                var modelsBmw = context.VehicleModels.Where(m => m.Brand == "BMW").ToList();
                var modelsMini = context.VehicleModels.Where(m => m.Brand == "Mini").ToList();

                foreach (var m in modelsBmw)
                {
                    decimal basePrice = m.ModelName switch
                    {
                        "Seria 1" => 145000m,
                        "Seria 3" => 195000m,
                        "Seria 5" => 285000m,
                        "X1" => 175000m,
                        "X3" => 240000m,
                        "X5" => 380000m,
                        _ => 200000m
                    };
                    context.TrimLevels.AddRange(
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Basic", BasePrice = basePrice },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Advantage", BasePrice = basePrice + 18000m },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "M-Sport", BasePrice = basePrice + 42000m }
                    );
                }
                foreach (var m in modelsMini)
                {
                    decimal basePrice = m.ModelName == "Countryman" ? 165000m : 130000m;
                    context.TrimLevels.AddRange(
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Classic", BasePrice = basePrice },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Sport", BasePrice = basePrice + 25000m }
                    );
                }
                context.SaveChanges();
            }

            // 8. Seeding Vehicles, Orders and Jobs
            if (!context.Vehicles.Any())
            {
                var trims = context.TrimLevels.ToList();
                var engines = context.Engines.ToList();
                var features = context.Features.Where(f => f.Category != "Kolor").ToList();
                var colors = context.Features.Where(f => f.Category == "Kolor").ToList();
                var clients = context.Clients.ToList();

                var allSellers = context.Workers.Where(w => w.User.RoleID == rSprzedawca.RoleID).ToList();
                var allMechanics = context.Workers.Where(w => w.User.RoleID == rSerwisant.RoleID).ToList();

                var vehiclesToInsert = new List<Vehicle>();
                var vehicleFeaturesToInsert = new List<VehicleFeature>();
                var ordersToInsert = new List<SalesOrder>();
                var jobsToInsert = new List<Job>();
                
                // Generate 250 vehicles (some sold, some available, some in progress)
                for (int i = 0; i < 250; i++)
                {
                    var ds = dealerships[rnd.Next(dealerships.Count)];
                    var trim = trims[rnd.Next(trims.Count)];
                    
                    var compatibleEngines = engines.Where(e => e.Brand == context.VehicleModels.First(m => m.ModelID == trim.ModelID).Brand).ToList();
                    var engine = compatibleEngines[rnd.Next(compatibleEngines.Count)];

                    // Status distribution: ~60% Sold (orders finished), ~20% Available, ~10% InProgress, ~10% Pending
                    int randStat = rnd.Next(100);
                    string vehicleStatus = "Dostępny";
                    string? orderStatus = null;
                    DateTime? orderDate = null;

                    if (randStat < 60)
                    {
                        vehicleStatus = "Sprzedany";
                        orderStatus = rnd.NextDouble() > 0.5 ? OrderStatuses.Finished : OrderStatuses.FinishedAlt;
                        orderDate = DateTime.Now.AddDays(-rnd.Next(1, 180)); // 6 months back
                    }
                    else if (randStat < 75)
                    {
                        vehicleStatus = "Dostępny"; // Not sold, maybe it was canceled previously? 
                        if (rnd.NextDouble() > 0.7) 
                        {
                            orderStatus = OrderStatuses.Canceled;
                            orderDate = DateTime.Now.AddDays(-rnd.Next(1, 100));
                        }
                    }
                    else if (randStat < 85)
                    {
                        vehicleStatus = "Zarezerwowany";
                        orderStatus = OrderStatuses.InProgress;
                        orderDate = DateTime.Now.AddDays(-rnd.Next(0, 14)); // recent
                    }
                    else
                    {
                        vehicleStatus = "Zarezerwowany";
                        orderStatus = OrderStatuses.Pending;
                        orderDate = DateTime.Now.AddDays(-rnd.Next(0, 5));
                    }

                    var auto = new Vehicle
                    {
                        VIN = $"WBA{rnd.Next(10000, 99999)}A{rnd.Next(1000000, 9999999)}",
                        TrimID = trim.TrimID,
                        EngineID = engine.EngineID,
                        Mileage = rnd.NextDouble() > 0.7 ? rnd.Next(5000, 150000) : rnd.Next(0, 50),
                        IsUsed = rnd.NextDouble() > 0.7,
                        DealershipID = ds.DealershipID,
                        Status = vehicleStatus
                    };
                    vehiclesToInsert.Add(auto);
                }
                context.Vehicles.AddRange(vehiclesToInsert);
                context.SaveChanges();

                // Now add features and orders
                decimal CalculateTotal(Vehicle v)
                {
                    var basePrice = context.TrimLevels.First(t => t.TrimID == v.TrimID).BasePrice;
                    var enginePrice = context.Engines.First(e => e.EngineID == v.EngineID).Price;
                    return basePrice + enginePrice;
                }

                foreach (var auto in vehiclesToInsert)
                {
                    decimal extraFeaturesPrice = 0;
                    
                    // Color
                    var color = colors[rnd.Next(colors.Count)];
                    vehicleFeaturesToInsert.Add(new VehicleFeature { VehicleID = auto.VehicleID, FeatureID = color.FeatureID, PurchasePrice = color.Price });
                    extraFeaturesPrice += color.Price;

                    // Extra features
                    int featureCount = rnd.Next(0, 4);
                    var selectedFeatures = features.OrderBy(x => rnd.Next()).Take(featureCount).ToList();
                    foreach (var f in selectedFeatures)
                    {
                        vehicleFeaturesToInsert.Add(new VehicleFeature { VehicleID = auto.VehicleID, FeatureID = f.FeatureID, PurchasePrice = f.Price });
                        extraFeaturesPrice += f.Price;
                    }

                    // Create Order if it has an orderStatus mapped in the initial loop
                    if (auto.Status == "Sprzedany" || auto.Status == "Zarezerwowany" || rnd.NextDouble() > 0.8)
                    {
                        string orderStatus = OrderStatuses.Finished;
                        DateTime orderDate = DateTime.Now.AddDays(-rnd.Next(1, 180));

                        if (auto.Status == "Sprzedany") orderStatus = rnd.NextDouble() > 0.5 ? OrderStatuses.Finished : OrderStatuses.FinishedAlt;
                        else if (auto.Status == "Zarezerwowany") orderStatus = rnd.NextDouble() > 0.5 ? OrderStatuses.InProgress : OrderStatuses.Pending;
                        else { orderStatus = OrderStatuses.Canceled; orderDate = DateTime.Now.AddDays(-rnd.Next(1, 100)); } // Available but had canceled order

                        if (auto.Status == "Zarezerwowany") orderDate = DateTime.Now.AddDays(-rnd.Next(0, 14)); // recent

                        var client = clients[rnd.Next(clients.Count)];
                        
                        // Select seller from the same dealership
                        var dsSellers = allSellers.Where(w => w.DealershipID == auto.DealershipID).ToList();
                        var seller = dsSellers.Any() ? dsSellers[rnd.Next(dsSellers.Count)] : allSellers[rnd.Next(allSellers.Count)];

                        var finalPrice = CalculateTotal(auto) + extraFeaturesPrice - rnd.Next(0, 15)*1000m; // some discount

                        var order = new SalesOrder
                        {
                            VehicleID = auto.VehicleID,
                            ClientID = client.ClientID,
                            WorkerID = seller.WorkerID,
                            OrderDate = orderDate,
                            FinalPrice = finalPrice > 0 ? finalPrice : 50000m,
                            Status = orderStatus,
                            DealershipID = auto.DealershipID
                        };
                        ordersToInsert.Add(order);
                    }
                }

                context.VehicleFeatures.AddRange(vehicleFeaturesToInsert);
                context.SalesOrders.AddRange(ordersToInsert);
                context.SaveChanges();

                // Generate Service Jobs
                foreach (var order in ordersToInsert)
                {
                    if (order.Status == OrderStatuses.Canceled) continue;

                    // 0-3 service jobs per order
                    int jobsCount = rnd.Next(0, 4);
                    var colorFeatureIds = colors.Select(c => c.FeatureID).ToList();
                    var orderFeatures = vehicleFeaturesToInsert
                        .Where(vf => vf.VehicleID == order.VehicleID && !colorFeatureIds.Contains(vf.FeatureID))
                        .Take(jobsCount)
                        .ToList();

                    var dsMechanics = allMechanics.Where(m => m.DealershipID == order.DealershipID).ToList();
                    var mechanic = dsMechanics.Any() ? dsMechanics[rnd.Next(dsMechanics.Count)] : allMechanics[rnd.Next(allMechanics.Count)];

                    bool allJobsDoneForThisActiveOrder = rnd.NextDouble() > 0.7; // 30% szans, że serwis już skończył robotę

                    foreach (var vf in orderFeatures)
                    {
                        string jobStatus = JobStatuses.Finished;
                        DateTime createdAt = order.OrderDate.AddDays(rnd.Next(0, 3));

                        if (order.Status == OrderStatuses.Pending || order.Status == OrderStatuses.InProgress)
                        {
                            if (allJobsDoneForThisActiveOrder)
                            {
                                jobStatus = JobStatuses.Finished;
                            }
                            else
                            {
                                jobStatus = rnd.NextDouble() > 0.5 ? JobStatuses.InProgress : JobStatuses.Pending;
                            }
                        }

                        jobsToInsert.Add(new Job
                        {
                            VehicleID = order.VehicleID,
                            WorkerID = mechanic.WorkerID,
                            FeatureID = vf.FeatureID,
                            Status = jobStatus,
                            CreatedAt = createdAt
                        });
                    }
                }
                
                context.Jobs.AddRange(jobsToInsert);
                context.SaveChanges();
            }
        }
    }
}