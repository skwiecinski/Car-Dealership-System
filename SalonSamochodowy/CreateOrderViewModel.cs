namespace SalonSamochodowy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class CreateOrderViewModel
{
    public bool CzyIstniejacyKlient { get; set; } = true;

    public ObservableCollection<KlientItem> ListaKlientow { get; set; } = new();
    public KlientItem? WybranyKlient { get; set; }

    public string NowyImie { get; set; } = "";
    public string NowyNazwisko { get; set; } = "";
    public string NowyEmail { get; set; } = "";
    public string NowyTelefon { get; set; } = "";
    public string NowyNIP { get; set; } = "";

    public ObservableCollection<string> Marki { get; set; } = new();
    public string? WybranaMarka { get; set; }

    public ObservableCollection<ModelItem> Modele { get; set; } = new();
    public ModelItem? WybranyModel { get; set; }

    public ObservableCollection<TrimItem> Wersje { get; set; } = new();
    public TrimItem? WybranaWersja { get; set; }

    public ObservableCollection<EngineItem> Silniki { get; set; } = new();
    public EngineItem? WybranySilnik { get; set; }

    public string Kolor { get; set; } = "";
    public string VIN { get; set; } = "";
    public bool CzyUzywany { get; set; } = false;
    public int Przebieg { get; set; } = 0;

    public ObservableCollection<DodatkowaOpcja> DodatkoweOpcje { get; set; } = new();

    public DateTime DataZamowienia { get; set; } = DateTime.Today;

    public ObservableCollection<SprzedawcaItem> ListaSprzedawcow { get; set; } = new();
    public SprzedawcaItem? WybranySprzedawca { get; set; }

    public List<string> FormyPlatnosci { get; set; } = new() { "Gotówka", "Kredyt", "Leasing" };
    public string? WybranaFormaPlatnosci { get; set; }

    public decimal CenaFinalna { get; set; } = 0m;

    public List<string> StatusyZamowienia { get; set; } = new() { "Nowe", "W realizacji", "Zrealizowane", "Anulowane" };
    public string WybranyStatus { get; set; } = "Nowe";

    public string Uwagi { get; set; } = "";

    public CreateOrderViewModel() { }
}

public class KlientItem
{
    public int ClientID { get; set; }
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public override string ToString() => string.IsNullOrEmpty(Email) ? FullName : $"{FullName} ({Email})";
}

public class ModelItem
{
    public int ModelID { get; set; }
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public override string ToString() => ModelName;
}

public class TrimItem
{
    public int TrimID { get; set; }
    public int ModelID { get; set; }
    public string TrimName { get; set; } = "";
    public decimal BasePrice { get; set; }
    public override string ToString() => $"{TrimName} ({BasePrice:N0} zł)";
}

public class EngineItem
{
    public int EngineID { get; set; }
    public string EngineName { get; set; } = "";
    public int Power { get; set; }
    public decimal Price { get; set; }
    public override string ToString() =>
        Price > 0 ? $"{EngineName} • {Power} KM (+{Price:N0} zł)" : $"{EngineName} • {Power} KM";
}

public class SprzedawcaItem
{
    public int WorkerID { get; set; }
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public override string ToString() => FullName;
}

public class DodatkowaOpcja
{
    public int FeatureID { get; set; }
    public string Nazwa { get; set; } = "";
    public string Kategoria { get; set; } = "";
    public bool Zaznaczona { get; set; }
}
