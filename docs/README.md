Salon Samochodowy

## Instalacja środowiska (Przejście na SQL Server)

**Instrukcja instalacji:**
1. Uruchom **Visual Studio Installer**.
2. Przy swojej wersji Visual Studio kliknij **Modify**.
3. Przejdź do zakładki **Pojedyncze składniki**
4. Wyszukaj i zaznacz **SQL Server Express 2019 LocalDB**
5. Kliknij *Modyfikuj/Zainstaluj*

## Pracowanie z bazą danych

Architektura jest opartana wzorcach **Repository** oraz **Unit of Work**. Dzięki temu nie musimy pisać zapytań SQL, cała komunikacja odbywa się za pomocoą dwóch mechanizmów.

* **`IRepository` / `Repository` **: Gotowe narzędzia do pracy z konkretną tabelą. Każdy repozytorium potrafi wykonać standardowe operacje: pobierz wszystko, pobierz jedno, dodaj, zaktualizuj, usuń.
* **`IUnitOfWork` / `UnitOfWork` **: Główny punkt styku z bazą danych. Ma pod sobą wszystkie repozytoria. Kiedy wykonujemy jakąś czynność to UnitOfWork tworzy zapytania SQL, a następnie wszystko zatwierdza.

### WAŻNE

> Metody takie jak `AddAsync`, `Update` czy `Delete` **NIE ZAPISUJĄ** zmian fizycznie w bazie! Jest to tylko zapisywane w pamięci podręcznej komputera. 
> Baza danych dowiaduje się o zmianach dopiero po **`await unitOfWork.CompleteAsync();`**

### Przykłady użycia w kodzie

#### 1. Odczytywanie danych 
```csharp
using (var dbContext = new AppDbContext())
using (var unitOfWork = new UnitOfWork(dbContext))
{
    // Pobranie wszystkich salonów
    var salony = await unitOfWork.Dealerships.GetAllAsync();

    // Pobranie konkretnego pracownika o ID = 5
    var pracownik = await unitOfWork.Workers.GetByIdAsync(5);
}
```

#### 2. Zapisywanie danych

```csharp
using (var dbContext = new AppDbContext())
using (var unitOfWork = new UnitOfWork(dbContext))
{
    // 1. Tworzenie nowego obiektu
    var nowyKlient = new Client { Phone = "123-456-789", UserID = 2 };

    // 2. Przekazanie go do repozytorium
    await unitOfWork.Clients.AddAsync(nowyKlient);

    // 3. Zapis fizyczny transakcji
    await unitOfWork.CompleteAsync(); 
}
```

#### 3. Edycja istniejących danych
```csharp
using (var dbContext = new AppDbContext())
using (var unitOfWork = new UnitOfWork(dbContext))
{
    // 1.Pobieranie auta z bazy
    var auto = await unitOfWork.Vehicles.GetByIdAsync(10);
    
    // 2. Zmiana przebiegu
    auto.Mileage = 150000;

    // 3. Informujemy system, że obiekt został zmieniony
    unitOfWork.Vehicles.Update(auto);

    // 4. Zatwierdzamy zmiany
    await unitOfWork.CompleteAsync();
}
```

#### 4. Usuwanie danych
```csharp
using (var dbContext = new AppDbContext())
using (var unitOfWork = new UnitOfWork(dbContext))
{
    var salon = await unitOfWork.Dealerships.GetByIdAsync(1);
    
    if (salon != null)
    {
        unitOfWork.Dealerships.Delete(salon);
        await unitOfWork.CompleteAsync();
    }
}
```

#### 5. Funkcja FindAsync
Funkcja, którą używamy aby odfiltrowała wyniki.

```csharp
using (var dbContext = new AppDbContext())
using (var unitOfWork = new UnitOfWork(dbContext))
{
    // Auta tylko, które są używane oraz są w salonu o ID 1.
    var dostepneUzywane = await unitOfWork.Vehicles.FindAsync(v => v.IsUsed == true && v.DealershipID == 1);

    // Znalezenie klientów o nazwisko "Kowalski"
    var kowalscy = await unitOfWork.Clients.FindAsync(c => c.LastName == "Kowalski");
}
```

### Dostępne funkcje
* `GetAllAsync()` -> Zwraca listę wszystkich elementów.
* `GetByIdAsync(int id)` -> Zwraca jeden element po jego ID.
* `FindAsync(warunek)` -> Filtruje dane bezpośrednio w bazie i zwraca tylko pasujące elementy.
* `AddAsync(T entity)` -> Czeka w kolejce na dopisanie nowego elementu.
* `Update(T entity)` -> Czeka w kolejce na zmianę istniejącego elementu.
* `Delete(T entity)` -> Czeka w kolejce na skasowanie elementu.
* `CompleteAsync()` -> **Wykonuje i zapisuje wszystko na dysku**
