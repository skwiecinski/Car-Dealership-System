namespace SalonSamochodowy;

using System;
using System.Collections.Generic;

public class CreateOrderViewModel
{
    // === Klient ===

    // Tryb wyboru: true = istniejacy klient, false = nowy klient
    public bool CzyIstniejacyKlient { get; set; } = true;

    // Lista istniejacych klientow - backend wypelni z bazy
    public List<string> ListaKlientow { get; set; } = new();
    public string? WybranyKlient { get; set; }

    // Pola nowego klienta
    public string NowyImie { get; set; } = "";
    public string NowyNazwisko { get; set; } = "";
    public string NowyEmail { get; set; } = "";
    public string NowyTelefon { get; set; } = "";
    public string NowyNIP { get; set; } = "";

    // === Pojazd ===

    // Salon dystrybuuje wylacznie BMW i Mini
    public List<string> Marki { get; set; } = new() { "BMW", "Mini" };
    public string? WybranaMarka { get; set; }

    // Modele - backend wypelni w zaleznosci od wybranej marki
    public List<string> Modele { get; set; } = new();
    public string? WybranyModel { get; set; }

    // Wersje wyposazenia (TrimLevel) - backend wypelni po wybraniu modelu
    public List<string> Wersje { get; set; } = new();
    public string? WybranaWersja { get; set; }

    // Silniki - backend wypelni
    public List<string> Silniki { get; set; } = new();
    public string? WybranySilnik { get; set; }

    public string Kolor { get; set; } = "";
    public string VIN { get; set; } = "";
    public bool CzyUzywany { get; set; } = false;
    public int Przebieg { get; set; } = 0;

    // === Wyposazenie dodatkowe (Features) ===

    // Backend wypelni - lista dostepnych opcji do zaznaczenia
    public List<DodatkowaOpcja> DodatkoweOpcje { get; set; } = new();

    // === Warunki zamowienia ===

    public DateTime DataZamowienia { get; set; } = DateTime.Today;

    public List<string> ListaSprzedawcow { get; set; } = new();
    public string? WybranySprzedawca { get; set; }

    public List<string> FormyPlatnosci { get; set; } = new() { "Gotówka", "Kredyt", "Leasing" };
    public string? WybranaFormaPlatnosci { get; set; }

    public decimal CenaFinalna { get; set; } = 0m;

    public List<string> StatusyZamowienia { get; set; } = new() { "Nowe", "W realizacji", "Zrealizowane", "Anulowane" };
    public string WybranyStatus { get; set; } = "Nowe";

    public string Uwagi { get; set; } = "";

    public CreateOrderViewModel() { }
}

public class DodatkowaOpcja
{
    public string Nazwa { get; set; } = "";
    public string Kategoria { get; set; } = "";
    public bool Zaznaczona { get; set; }
}
