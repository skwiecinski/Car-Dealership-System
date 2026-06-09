using System;
using System.Linq;
using SalonSamochodowy.Entities;
using SalonSamochodowy.Services;

namespace SalonSamochodowy
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.AppRoles.Any())
            {
                context.AppRoles.AddRange(
                    new AppRole { RoleName = "Administrator", PermissionLevel = 0 },
                    new AppRole { RoleName = "Kierownik", PermissionLevel = 1 },
                    new AppRole { RoleName = "Sprzedawca", PermissionLevel = 2 },
                    new AppRole { RoleName = "Serwisant", PermissionLevel = 3 },
                    new AppRole { RoleName = "Klient", PermissionLevel = 4 }
                );
                context.SaveChanges();
            }

            if (!context.AppUsers.Any())
            {
                var rAdmin = context.AppRoles.First(r => r.RoleName == "Administrator");
                var rKierownik = context.AppRoles.First(r => r.RoleName == "Kierownik");
                var rSprzedawca = context.AppRoles.First(r => r.RoleName == "Sprzedawca");
                var rSerwisant = context.AppRoles.First(r => r.RoleName == "Serwisant");
                var rKlient = context.AppRoles.First(r => r.RoleName == "Klient");

                context.AppUsers.AddRange(
                    new AppUser { FirstName = "Anna", LastName = "Adminowa", Email = "admin@salon.pl", PasswordHash = AuthService.HashPassword("admin123"), RoleID = rAdmin.RoleID, BirthDate = new DateTime(1980, 1, 15) },
                    new AppUser { FirstName = "Wiesław", LastName = "Kierowniczy", Email = "kierownik@salon.pl", PasswordHash = AuthService.HashPassword("kierownik123"), RoleID = rKierownik.RoleID, BirthDate = new DateTime(1977, 12, 3) },
                    new AppUser { FirstName = "Tomasz", LastName = "Sprzedażowy", Email = "sprzedawca@salon.pl", PasswordHash = AuthService.HashPassword("sprzedawca123"), RoleID = rSprzedawca.RoleID, BirthDate = new DateTime(1990, 5, 10) },
                    new AppUser { FirstName = "Piotr", LastName = "Serwisowy", Email = "serwis@salon.pl", PasswordHash = AuthService.HashPassword("serwis123"), RoleID = rSerwisant.RoleID, BirthDate = new DateTime(1985, 8, 20) },
                    new AppUser { FirstName = "Jan", LastName = "Kowalski", Email = "klient@wp.pl", PasswordHash = AuthService.HashPassword("klient123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(1995, 2, 14) }
                );
                context.SaveChanges();
            }

            if (!context.Dealerships.Any())
            {
                context.Dealerships.Add(new Dealership { Name = "Główny Salon", Address = "ul. Akademicka 16", City = "Gliwice", Owner = "Sigma Boy" });
                context.SaveChanges();
            }

            if (!context.Workers.Any() && !context.Clients.Any())
            {
                var rKierownik = context.AppRoles.First(r => r.RoleName == "Kierownik");
                var rSprzedawca = context.AppRoles.First(r => r.RoleName == "Sprzedawca");
                var rSerwisant = context.AppRoles.First(r => r.RoleName == "Serwisant");

                context.AppUsers.AddRange(
                    new AppUser { FirstName = "Robert", LastName = "Nowy-Kierownik", Email = "kierownik2@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rKierownik.RoleID, BirthDate = new DateTime(1982, 4, 11) },
                    new AppUser { FirstName = "Karolina", LastName = "Bystra", Email = "sprzedawca2@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rSprzedawca.RoleID, BirthDate = new DateTime(1993, 7, 22) },
                    new AppUser { FirstName = "Michał", LastName = "Dobry", Email = "sprzedawca3@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rSprzedawca.RoleID, BirthDate = new DateTime(1995, 11, 5) },
                    new AppUser { FirstName = "Dawid", LastName = "Klucz", Email = "serwis2@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rSerwisant.RoleID, BirthDate = new DateTime(1988, 1, 30) },
                    new AppUser { FirstName = "Krzysztof", LastName = "Smar", Email = "serwis3@salon.pl", PasswordHash = AuthService.HashPassword("123"), RoleID = rSerwisant.RoleID, BirthDate = new DateTime(1991, 9, 15) }
                );
                context.SaveChanges();

                var uKierownik = context.AppUsers.First(u => u.Email == "kierownik@salon.pl");
                var uSprzedawca = context.AppUsers.First(u => u.Email == "sprzedawca@salon.pl");
                var uSerwisant = context.AppUsers.First(u => u.Email == "serwis@salon.pl");
                var uKlient = context.AppUsers.First(u => u.Email == "klient@wp.pl");
                
                var newKierownik = context.AppUsers.First(u => u.Email == "kierownik2@salon.pl");
                var newSprzedawca1 = context.AppUsers.First(u => u.Email == "sprzedawca2@salon.pl");
                var newSprzedawca2 = context.AppUsers.First(u => u.Email == "sprzedawca3@salon.pl");
                var newSerwisant1 = context.AppUsers.First(u => u.Email == "serwis2@salon.pl");
                var newSerwisant2 = context.AppUsers.First(u => u.Email == "serwis3@salon.pl");

                var salon = context.Dealerships.First();

                context.Workers.AddRange(
                    new Worker { UserID = uKierownik.UserID,  Payroll = 9000m, EndOfContractDate = new DateTime(2028, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = uSprzedawca.UserID, Payroll = 6000m, EndOfContractDate = new DateTime(2027, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = uSerwisant.UserID,  Payroll = 5500m, EndOfContractDate = new DateTime(2026, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = newKierownik.UserID, Payroll = 8500m, EndOfContractDate = new DateTime(2028, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = newSprzedawca1.UserID, Payroll = 5800m, EndOfContractDate = new DateTime(2027, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = newSprzedawca2.UserID, Payroll = 5900m, EndOfContractDate = new DateTime(2027, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = newSerwisant1.UserID, Payroll = 5200m, EndOfContractDate = new DateTime(2026, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = newSerwisant2.UserID, Payroll = 5400m, EndOfContractDate = new DateTime(2026, 12, 31), DealershipID = salon.DealershipID }
                );

                context.Clients.Add(new Client { UserID = uKlient.UserID, NIP = "1234567890", Phone = "987-654-321" });
                context.SaveChanges();
            }

            if (!context.Engines.Any())
            {
                context.Engines.AddRange(
                    new Engine { Brand = "BMW",  EngineName = "1.5 TwinPower Turbo",         EngineSize = "1.5L", Power = 140, Price = 0m },
                    new Engine { Brand = "BMW",  EngineName = "2.0 TwinPower Turbo Benzyna", EngineSize = "2.0L", Power = 184, Price = 8000m },
                    new Engine { Brand = "BMW",  EngineName = "2.0 TwinPower Turbo Diesel",  EngineSize = "2.0L", Power = 190, Price = 10000m },
                    new Engine { Brand = "BMW",  EngineName = "3.0 TwinPower Turbo Diesel",  EngineSize = "3.0L", Power = 286, Price = 22000m },
                    new Engine { Brand = "BMW",  EngineName = "3.0 TwinPower Turbo Benzyna", EngineSize = "3.0L", Power = 333, Price = 24000m },
                    new Engine { Brand = "BMW",  EngineName = "M xDrive 4.4 V8",             EngineSize = "4.4L", Power = 530, Price = 60000m },
                    new Engine { Brand = "Mini", EngineName = "1.5 Cooper",                  EngineSize = "1.5L", Power = 136, Price = 0m },
                    new Engine { Brand = "Mini", EngineName = "2.0 Cooper S",                EngineSize = "2.0L", Power = 178, Price = 9000m },
                    new Engine { Brand = "Mini", EngineName = "2.0 John Cooper Works",       EngineSize = "2.0L", Power = 231, Price = 18000m }
                );
                context.SaveChanges();
            }

            if (!context.VehicleModels.Any())
            {
                context.VehicleModels.AddRange(
                    new VehicleModel { Brand = "BMW",  ModelName = "Seria 1" },
                    new VehicleModel { Brand = "BMW",  ModelName = "Seria 3" },
                    new VehicleModel { Brand = "BMW",  ModelName = "Seria 5" },
                    new VehicleModel { Brand = "BMW",  ModelName = "X1" },
                    new VehicleModel { Brand = "BMW",  ModelName = "X3" },
                    new VehicleModel { Brand = "BMW",  ModelName = "X5" },
                    new VehicleModel { Brand = "Mini", ModelName = "Cooper" },
                    new VehicleModel { Brand = "Mini", ModelName = "Countryman" }
                );
                context.SaveChanges();
            }

            if (!context.Features.Any())
            {
                context.Features.AddRange(
                    new Feature { FeatureName = "Lakier metalik",        Category = "Wygląd", Price = 3000m },
                    new Feature { FeatureName = "Czujniki parkowania",   Category = "Akcesoria", Price = 5000m },
                    new Feature { FeatureName = "Kamera cofania",        Category = "Akcesoria", Price = 2000m },
                    new Feature { FeatureName = "Alarm",                 Category = "Bezpieczeństwo", Price = 1500m },
                    new Feature { FeatureName = "Klimatyzacja 2-strefowa", Category = "Komfort", Price = 2500m },
                    new Feature { FeatureName = "Skórzane fotele",       Category = "Wnętrze", Price = 6000m },
                    new Feature { FeatureName = "Nawigacja",             Category = "Multimedia", Price = 4000m },
                    new Feature { FeatureName = "Pakiet sportowy M",     Category = "Wygląd", Price = 12000m }
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
                        "X1"      => 175000m,
                        "X3"      => 240000m,
                        "X5"      => 380000m,
                        _ => 200000m
                    };
                    context.TrimLevels.AddRange(
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Basic",     BasePrice = basePrice },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Advantage", BasePrice = basePrice + 18000m },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "M-Sport",   BasePrice = basePrice + 42000m }
                    );
                }
                foreach (var m in modelsMini)
                {
                    decimal basePrice = m.ModelName == "Countryman" ? 165000m : 130000m;
                    context.TrimLevels.AddRange(
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Classic", BasePrice = basePrice },
                        new TrimLevel { ModelID = m.ModelID, TrimName = "Sport",   BasePrice = basePrice + 25000m }
                    );
                }
                context.SaveChanges();
            }

            if (!context.Vehicles.Any())
            {
                var salon = context.Dealerships.First();
                var silnik = context.Engines.First();
                var trim = context.TrimLevels.First();

                var auto = new Vehicle { VIN = "1FA6P8CF7L1234567", TrimID = trim.TrimID, EngineID = silnik.EngineID, Mileage = 10, IsUsed = false, DealershipID = salon.DealershipID, Status = "Dostępny" };
                context.Vehicles.Add(auto);
                context.SaveChanges();

                var featurePark = context.Features.First(f => f.FeatureName == "Czujniki parkowania");
                context.VehicleFeatures.Add(new VehicleFeature { VehicleID = auto.VehicleID, FeatureID = featurePark.FeatureID, PurchasePrice = 1500m });
                context.SaveChanges();
            }

            if (!context.SalesOrders.Any())
            {
                var auto = context.Vehicles.First();
                var klient = context.Clients.First();
                var salon = context.Dealerships.First();

                var uSprzedawca = context.AppUsers.First(u => u.Email == "sprzedawca@salon.pl");
                var uSerwisant = context.AppUsers.First(u => u.Email == "serwis@salon.pl");
                var wSprzedawca = context.Workers.First(w => w.UserID == uSprzedawca.UserID);
                var wSerwisant = context.Workers.First(w => w.UserID == uSerwisant.UserID);

                var featurePark = context.Features.First(f => f.FeatureName == "Czujniki parkowania");

                context.SalesOrders.Add(new SalesOrder
                {
                    VehicleID = auto.VehicleID,
                    ClientID = klient.ClientID,
                    WorkerID = wSprzedawca.WorkerID,
                    OrderDate = DateTime.Now,
                    FinalPrice = 251500m,
                    Status = "W realizacji",
                    DealershipID = salon.DealershipID
                });

                context.Jobs.Add(new Job
                {
                    VehicleID = auto.VehicleID,
                    WorkerID = wSerwisant.WorkerID,
                    FeatureID = featurePark.FeatureID,
                    Status = "Oczekujące",
                    CreatedAt = DateTime.Now
                });

                context.SaveChanges();
            }

            if (!context.Features.Any(f => f.Category == "Kolor"))
            {
                context.Features.AddRange(
                    new Feature { FeatureName = "Alpejska Biel (bazowy)", Category = "Kolor", Price = 0m },
                    new Feature { FeatureName = "Czarny Szafir metalik",   Category = "Kolor", Price = 3500m },
                    new Feature { FeatureName = "Szary Melbourne metalik",Category = "Kolor", Price = 3500m },
                    new Feature { FeatureName = "Niebieski Phytonic metalik", Category = "Kolor", Price = 4000m }
                );
                context.SaveChanges();
            }

            var baseColor = context.Features.FirstOrDefault(f => f.FeatureName == "Alpejska Biel (bazowy)");
            if (baseColor != null)
            {
                var vehiclesWithoutColor = context.Vehicles
                    .Where(v => !context.VehicleFeatures.Any(vf => vf.VehicleID == v.VehicleID && vf.Feature.Category == "Kolor"))
                    .ToList();

                foreach (var v in vehiclesWithoutColor)
                {
                    context.VehicleFeatures.Add(new VehicleFeature
                    {
                        VehicleID = v.VehicleID,
                        FeatureID = baseColor.FeatureID,
                        PurchasePrice = 0m
                    });
                }
                context.SaveChanges();
            }

            if (context.SalesOrders.Count() < 10)
            {
                var rKlient = context.AppRoles.First(r => r.RoleName == "Klient");
                var uSprzedawca = context.AppUsers.First(u => u.Email == "sprzedawca@salon.pl");
                var uSerwisant = context.AppUsers.First(u => u.Email == "serwis@salon.pl");
                var wSprzedawca = context.Workers.First(w => w.UserID == uSprzedawca.UserID);
                var wSerwisant = context.Workers.First(w => w.UserID == uSerwisant.UserID);

                var salon = context.Dealerships.First();
                var trims = context.TrimLevels.ToList();
                var engines = context.Engines.ToList();
                var features = context.Features.ToList();

                var klienciUsers = new List<AppUser>
                {
                    new AppUser { FirstName = "Marek", LastName = "Nowak", Email = "m.nowak@gmail.com", PasswordHash = AuthService.HashPassword("klient123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(1980, 5, 5) },
                    new AppUser { FirstName = "Ewa", LastName = "Wiśniewska", Email = "ewa.w@wp.pl", PasswordHash = AuthService.HashPassword("klient123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(1992, 11, 10) },
                    new AppUser { FirstName = "Piotr", LastName = "Zieliński", Email = "piotrz@onet.pl", PasswordHash = AuthService.HashPassword("klient123"), RoleID = rKlient.RoleID, BirthDate = new DateTime(1975, 2, 20) }
                };
                context.AppUsers.AddRange(klienciUsers);
                context.SaveChanges();

                var klienci = new List<Client>
                {
                    new Client { UserID = klienciUsers[0].UserID, Phone = "111-222-333" },
                    new Client { UserID = klienciUsers[1].UserID, Phone = "444-555-666" },
                    new Client { UserID = klienciUsers[2].UserID, Phone = "777-888-999" }
                };
                context.Clients.AddRange(klienci);
                context.SaveChanges();

                var allClients = context.Clients.ToList();
                Random rand = new Random(1234); // stałe ziarno dla powtarzalności

                for (int i = 1; i <= 30; i++)
                {
                    var trim = trims[rand.Next(trims.Count)];
                    var engine = engines[rand.Next(engines.Count)];

                    var auto = new Vehicle
                    {
                        VIN = $"WBA{rand.Next(10000, 99999)}A{rand.Next(1000000, 9999999)}",
                        TrimID = trim.TrimID,
                        EngineID = engine.EngineID,
                        Mileage = rand.Next(0, 150000),
                        IsUsed = rand.NextDouble() > 0.5,
                        DealershipID = salon.DealershipID,
                        Status = "Sprzedany"
                    };
                    context.Vehicles.Add(auto);
                    context.SaveChanges();

                    var date = DateTime.Now.AddMonths(-rand.Next(0, 12)).AddDays(-rand.Next(1, 28));

                    var order = new SalesOrder
                    {
                        VehicleID = auto.VehicleID,
                        ClientID = allClients[rand.Next(allClients.Count)].ClientID,
                        WorkerID = wSprzedawca.WorkerID,
                        OrderDate = date,
                        FinalPrice = trim.BasePrice + engine.Price + rand.Next(5000, 20000),
                        Status = OrderStatuses.Finished,
                        DealershipID = salon.DealershipID
                    };
                    context.SalesOrders.Add(order);

                    var numJobs = rand.Next(1, 4);
                    for (int j = 0; j < numJobs; j++)
                    {
                        context.Jobs.Add(new Job
                        {
                            VehicleID = auto.VehicleID,
                            WorkerID = wSerwisant.WorkerID,
                            FeatureID = features[rand.Next(features.Count)].FeatureID,
                            Status = JobStatuses.Finished,
                            CreatedAt = date.AddDays(-rand.Next(1, 5))
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}