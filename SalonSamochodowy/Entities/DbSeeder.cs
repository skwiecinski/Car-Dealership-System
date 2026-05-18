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
                    new AppRole { RoleName = "Kierownik", PermissionLevel = 1 },
                    new AppRole { RoleName = "Sprzedawca", PermissionLevel = 2 },
                    new AppRole { RoleName = "Serwisant", PermissionLevel = 3 },
                    new AppRole { RoleName = "Klient", PermissionLevel = 4 }
                );
                context.SaveChanges();
            }

            if (!context.AppUsers.Any())
            {
                var rKierownik = context.AppRoles.First(r => r.RoleName == "Kierownik");
                var rSprzedawca = context.AppRoles.First(r => r.RoleName == "Sprzedawca");
                var rSerwisant = context.AppRoles.First(r => r.RoleName == "Serwisant");
                var rKlient = context.AppRoles.First(r => r.RoleName == "Klient");

                context.AppUsers.AddRange(
                    new AppUser { FirstName = "Adam", LastName = "Małysz", Email = "admin@salon.pl", PasswordHash = "admin123", RoleID = rKierownik.RoleID, BirthDate = new DateTime(1977, 12, 3) },
                    new AppUser { FirstName = "Tomasz", LastName = "Sprzedażowy", Email = "sprzedawca@salon.pl", PasswordHash = "haslo123", RoleID = rSprzedawca.RoleID, BirthDate = new DateTime(1990, 5, 10) },
                    new AppUser { FirstName = "Piotr", LastName = "Klucz", Email = "serwis@salon.pl", PasswordHash = "haslo123", RoleID = rSerwisant.RoleID, BirthDate = new DateTime(1985, 8, 20) },
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
                context.Engines.Add(new Engine { EngineName = "5.0 V8 Coyote", EngineSize = "5.0L", Power = 450, Price = 40000m });
                context.VehicleModels.Add(new VehicleModel { Brand = "Ford", ModelName = "Mustang" });
                context.Features.AddRange(
                    new Feature { FeatureName = "Lakier Metalik", Category = "Wygląd" },
                    new Feature { FeatureName = "Czujniki Parkowania", Category = "Akcesoria" }
                );
                context.SaveChanges();
            }

            if (!context.TrimLevels.Any())
            {
                var model = context.VehicleModels.First();
                var featureColor = context.Features.First(f => f.FeatureName == "Lakier Metalik");

                var trim = new TrimLevel { ModelID = model.ModelID, TrimName = "GT Fastback", BasePrice = 250000m };
                context.TrimLevels.Add(trim);
                context.SaveChanges();

                context.TrimFeatures.Add(new TrimFeature { TrimID = trim.TrimID, FeatureID = featureColor.FeatureID, IsStandard = true, AdditionalPrice = 0m });
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

                var featurePark = context.Features.First(f => f.FeatureName == "Czujniki Parkowania");
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

                var featurePark = context.Features.First(f => f.FeatureName == "Czujniki Parkowania");

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