using System;
using System.Linq;
using SalonSamochodowy.Entities;

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
                    new AppUser { FirstName = "Anna", LastName = "Adminowa", Email = "administrator@salon.pl", PasswordHash = "admin", RoleID = rAdmin.RoleID, BirthDate = new DateTime(1980, 1, 15) },
                    new AppUser { FirstName = "Wiesław", LastName = "Kierowniczy", Email = "admin@salon.pl", PasswordHash = "admin123", RoleID = rKierownik.RoleID, BirthDate = new DateTime(1977, 12, 3) },
                    new AppUser { FirstName = "Tomasz", LastName = "Sprzedażowy", Email = "sprzedawca@salon.pl", PasswordHash = "haslo123", RoleID = rSprzedawca.RoleID, BirthDate = new DateTime(1990, 5, 10) },
                    new AppUser { FirstName = "Piotr", LastName = "Serwisowy", Email = "serwis@salon.pl", PasswordHash = "haslo123", RoleID = rSerwisant.RoleID, BirthDate = new DateTime(1985, 8, 20) },
                    new AppUser { FirstName = "Jan", LastName = "Kowalski", Email = "klient@wp.pl", PasswordHash = "klient123", RoleID = rKlient.RoleID, BirthDate = new DateTime(1995, 2, 14) }
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
                var uSprzedawca = context.AppUsers.First(u => u.Email == "sprzedawca@salon.pl");
                var uSerwisant = context.AppUsers.First(u => u.Email == "serwis@salon.pl");
                var uKlient = context.AppUsers.First(u => u.Email == "klient@wp.pl");
                var salon = context.Dealerships.First();

                context.Workers.AddRange(
                    new Worker { UserID = uSprzedawca.UserID, Payroll = 6000m, EndOfContractDate = new DateTime(2027, 12, 31), DealershipID = salon.DealershipID },
                    new Worker { UserID = uSerwisant.UserID, Payroll = 5500m, EndOfContractDate = new DateTime(2026, 12, 31), DealershipID = salon.DealershipID }
                );

                context.Clients.Add(new Client { UserID = uKlient.UserID, NIP = "1234567890", Phone = "987-654-321" });
                context.SaveChanges();
            }

            if (!context.Engines.Any())
            {
                context.Engines.AddRange(
                    new Engine { EngineName = "1.5 TwinPower Turbo",         EngineSize = "1.5L", Power = 140, Price = 0m },
                    new Engine { EngineName = "2.0 TwinPower Turbo Benzyna", EngineSize = "2.0L", Power = 184, Price = 8000m },
                    new Engine { EngineName = "2.0 TwinPower Turbo Diesel",  EngineSize = "2.0L", Power = 190, Price = 10000m },
                    new Engine { EngineName = "3.0 TwinPower Turbo Diesel",  EngineSize = "3.0L", Power = 286, Price = 22000m },
                    new Engine { EngineName = "3.0 TwinPower Turbo Benzyna", EngineSize = "3.0L", Power = 333, Price = 24000m },
                    new Engine { EngineName = "M xDrive 4.4 V8",             EngineSize = "4.4L", Power = 530, Price = 60000m }
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
                    new Feature { FeatureName = "Lakier metalik",        Category = "Wygląd" },
                    new Feature { FeatureName = "Czujniki parkowania",   Category = "Akcesoria" },
                    new Feature { FeatureName = "Kamera cofania",        Category = "Akcesoria" },
                    new Feature { FeatureName = "Alarm",                 Category = "Bezpieczeństwo" },
                    new Feature { FeatureName = "Klimatyzacja 2-strefowa", Category = "Komfort" },
                    new Feature { FeatureName = "Skórzane fotele",       Category = "Wnętrze" },
                    new Feature { FeatureName = "Nawigacja",             Category = "Multimedia" },
                    new Feature { FeatureName = "Pakiet sportowy M",     Category = "Wygląd" }
                );
                context.SaveChanges();
            }

            if (!context.TrimLevels.Any())
            {
                // Dla kazdego modelu BMW: Basic, Advantage, M-Sport
                // Dla kazdego modelu Mini: Classic, Sport
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
                    Status = "W trakcie",
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
        }
    }
}