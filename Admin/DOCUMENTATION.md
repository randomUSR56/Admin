# OnlyFix Admin — Műszaki Dokumentáció

## Összefoglaló

Az **OnlyFix Admin** egy platformfüggetlen mobil/asztali adminisztrációs panel, amely **.NET 10 MAUI** keretrendszerrel készült. Egy **Laravel REST API backendhez** csatlakozik (amelyet a `http://localhost:8000` címen vár), és lehetővé teszi a hitelesített adminisztrátorok számára az OnlyFix platform teljes körű kezelését. Az alkalmazás támogatja a Bearer token alapú bejelentkezést, egy irányítópultot élő szerver-állapotellenőrzéssel és összesített statisztikákkal, valamint teljes CRUD (Létrehozás, Olvasás, Frissítés, Törlés) műveleteket **felhasználókon**, **járműveken**, **hibákon** és **szervizjegyeken** — lapozott kereséssel, szűrőkkel és jegy munkafolyamat-műveletekkel. A megoldáshoz **xUnit-alapú tesztprojekt** is tartozik, amely lefedi a modelleket, a kérés-DTO-kat és az API-kliens logikát.

---

## Tartalomjegyzék

1. [Technológiai Stack](#technológiai-stack)
2. [Projektstruktúra](#projektstruktúra)
3. [Architektúra Áttekintés](#architektúra-áttekintés)
4. [Alkalmazás Indítása és Navigáció](#alkalmazás-indítása-és-navigáció)
5. [Külső API Integráció](#külső-api-integráció)
6. [Hitelesítés](#hitelesítés)
7. [Képernyők és Funkciók](#képernyők-és-funkciók)
8. [Adatmodellek](#adatmodellek)
9. [Érték Konverterek](#érték-konverterek)
10. [Téma és Stílus](#téma-és-stílus)
11. [Tesztelés](#tesztelés)
12. [Elért Eredmények](#elért-eredmények)

---

## Technológiai Stack

| Réteg         | Technológia                                                        |
| ------------- | ------------------------------------------------------------------ |
| Keretrendszer | .NET 10 MAUI (Multi-platform App UI)                               |
| UI Minta      | MVVM `CommunityToolkit.Mvvm` használatával                         |
| HTTP Kliens   | `System.Net.Http.HttpClient` (típusos, DI-kezelt)                  |
| Szerializáció | `System.Text.Json`                                                 |
| Token Tárolás | `Microsoft.Maui.Storage.Preferences`                               |
| XAML Fordítás | Forrásgenerátor (`MauiXamlInflator=SourceGen`)                     |
| Célplatformok | Android, iOS, macOS Catalyst, Windows                              |
| Backend       | Laravel PHP REST API (Bearer token Sanctum/Passport használatával) |

---

## Projektstruktúra

```
Admin/                               # MAUI alkalmazásprojekt
├── App.xaml / App.xaml.cs           # Alkalmazás belépési pont, indítási útválasztás
├── AppShell.xaml / AppShell.xaml.cs  # Shell navigációs struktúra (Flyout oldalsáv)
├── MauiProgram.cs                   # DI konténer beállítás
│
├── Models/
│   ├── AuthModels.cs                # LoginRequest, LoginResponse, ApiErrorResponse
│   ├── User.cs                      # User és Role modellek
│   ├── UserRequests.cs              # CreateUserRequest, UpdateUserRequest
│   ├── Car.cs                       # Car modell
│   ├── CarRequests.cs               # CreateCarRequest, UpdateCarRequest
│   ├── Problem.cs                   # Problem és ProblemPivot modellek
│   ├── ProblemRequests.cs           # CreateProblemRequest, UpdateProblemRequest
│   ├── ProblemStatistics.cs         # ProblemStatistics, ProblemFrequency
│   ├── Ticket.cs                    # Ticket modell (munkafolyamat jelzőkkel)
│   ├── TicketRequests.cs            # CreateTicketRequest, UpdateTicketRequest
│   ├── TicketStatistics.cs          # TicketStatistics, TicketStatusCounts, TicketPriorityCounts
│   └── PaginatedResponse.cs         # Általános lapozott API wrapper
│
├── Services/
│   ├── IApiClient.cs                # API kliens interfész
│   ├── ApiClient.cs                 # Összes HTTP hívás a Laravel backendhez
│   ├── ApiException.cs              # Egyedi kivétel HTTP állapotkóddal és validációs hibákkal
│   └── AuthTokenStore.cs            # Auth token és felhasználói adatok mentése/lekérése
│
├── ViewModels/
│   ├── LoginViewModel.cs            # Bejelentkezési űrlap logika
│   ├── DashboardViewModel.cs        # Irányítópult statisztikák és navigáció
│   ├── UsersViewModel.cs            # Felhasználólista, keresés, szűrés, lapozás, törlés
│   ├── UserDetailViewModel.cs       # Felhasználó létrehozása / szerkesztése
│   ├── CarsViewModel.cs             # Járműlista, keresés, lapozás, törlés
│   ├── CarDetailViewModel.cs        # Jármű létrehozása / szerkesztése
│   ├── ProblemsViewModel.cs         # Hiba-lista, keresés, kategória szűrő, lapozás, törlés
│   ├── ProblemDetailViewModel.cs    # Hiba létrehozása / szerkesztése
│   ├── TicketsViewModel.cs          # Jegylista, státusz/prioritás szűrő, lapozás, törlés
│   └── TicketDetailViewModel.cs     # Jegy létrehozása / szerkesztése + munkafolyamat műveletek
│
├── Views/
│   ├── LoginPage.xaml               # Bejelentkezési űrlap
│   ├── DashboardPage.xaml           # Áttekintés + szerver állapot + statisztikák
│   ├── UsersPage.xaml               # Lapozott felhasználólista
│   ├── UserDetailPage.xaml          # Felhasználó létrehozó / szerkesztő űrlap
│   ├── CarsPage.xaml                # Lapozott járműlista
│   ├── CarDetailPage.xaml           # Jármű létrehozó / szerkesztő űrlap
│   ├── ProblemsPage.xaml            # Lapozott hiba-lista
│   ├── ProblemDetailPage.xaml       # Hiba létrehozó / szerkesztő űrlap
│   ├── TicketsPage.xaml             # Lapozott jegylista
│   └── TicketDetailPage.xaml        # Jegy részletei + munkafolyamat gombok
│
├── Converters/
│   └── Converters.cs               # InvertBoolConverter, BusyTextConverter,
│                                   # StatusColorConverter, StatusTextConverter
│
└── Resources/
    └── Styles/
        └── Colors.xaml             # Globális színpaletta

Admin.Tests/                         # xUnit tesztprojekt
├── Admin.Tests.csproj
├── Models/
│   ├── CarModelTests.cs             # Car modell és deszializáció tesztek
│   ├── CarRequestsTests.cs          # CreateCarRequest / UpdateCarRequest tesztek
│   ├── ProblemModelTests.cs         # Problem modell és ActiveDisplay tesztek
│   ├── ProblemRequestsTests.cs      # CreateProblemRequest / UpdateProblemRequest tesztek
│   ├── ProblemStatisticsTests.cs    # ProblemStatistics deszializáció tesztek
│   ├── TicketModelTests.cs          # Ticket modell, munkafolyamat jelzők, display tesztek
│   ├── TicketRequestsTests.cs       # CreateTicketRequest / UpdateTicketRequest tesztek
│   └── TicketStatisticsTests.cs     # TicketStatistics deszializáció tesztek
├── Services/
│   └── ApiExceptionTests.cs         # ApiException jelzők és üzenet tesztek
└── ViewModels/
    ├── CarsApiClientTests.cs         # ApiClient jármű végpont tesztek (Moq)
    ├── ProblemsApiClientTests.cs     # ApiClient hiba végpont tesztek (Moq)
    └── TicketsApiClientTests.cs      # ApiClient jegy végpont tesztek (Moq)
```

---

## Architektúra Áttekintés

Az alkalmazás szigorú **MVVM** mintát követ:

- A **View-k** tiszta XAML-ek fordított kötésekkel (`x:DataType`). Nem tartalmaznak üzleti logikát.
- A **ViewModel-ek** tartalmazzák az összes állapotot (`[ObservableProperty]` segítségével) és az összes parancsot (`[RelayCommand]` segítségével), amelyek a `CommunityToolkit.Mvvm` csomagból származnak. Szolgáltatásokkal kommunikálnak, de soha nem hivatkoznak közvetlenül a nézetre.
- A **Szolgáltatások** (`ApiClient`, `AuthTokenStore`) a .NET MAUI beépített DI konténerén keresztül kerülnek injektálásra, amelyek a `MauiProgram.cs` fájlban vannak regisztrálva.

```
View  <──kötések──>  ViewModel  <──DI──>  ApiClient  <──HTTP──>  Laravel API
                                    └──DI──>  AuthTokenStore
```

A ViewModel-ek **Transient** (navigációnként új példány) módon vannak regisztrálva, míg az `AuthTokenStore` **Singleton** (az alkalmazás teljes élettartamára megosztott). Az `ApiClient` az `AddHttpClient<T>` segítségével van regisztrálva, amely egy kezelt `HttpClient`-et hoz létre előre konfigurált alap URL-lel és `Accept: application/json` fejléccel.

---

## Alkalmazás Indítása és Navigáció

### `MauiProgram.cs`

Inicializálja a DI konténert. Regisztrálja:

- `AuthTokenStore`-t singleton-ként
- `ApiClient`-et típusos `HttpClient`-ként `http://localhost:8000` alap URL-lel
- Az összes ViewModel-t transient-ként
- Az összes Page-et transient-ként

### `App.xaml.cs`

A `Window.Loaded` eseményben ellenőrzi, hogy létezik-e token az `AuthTokenStore`-ban:

- **Token megtalálva** → navigál a `//Dashboard/DashboardPage` útvonalra (átugorja a bejelentkezést)
- **Nincs token** → navigál a `//LoginPage` útvonalra

### `AppShell.xaml`

Egy `LoginPage` `ShellContent` (navigációs sáv és flyout nélkül) és öt `FlyoutItem` szekciót definiál:

| Útvonal                                                    | Leírás                                                                       |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------- |
| `//LoginPage`                                              | Bejelentkezési képernyő (nincs navigációs sáv, nincs flyout elem)            |
| `//Dashboard/DashboardPage`                                | Irányítópult flyout elem                                                     |
| `//Users/UsersPage`                                        | Felhasználók flyout elem                                                     |
| `//Cars/CarsPage`                                          | Járművek flyout elem                                                         |
| `//Problems/ProblemsPage`                                  | Hibák flyout elem                                                            |
| `//Tickets/TicketsPage`                                    | Szervizjegyek flyout elem                                                    |
| `UserDetail`, `CarDetail`, `ProblemDetail`, `TicketDetail` | Shell útvonalként regisztrálva (push navigáció) létrehozáshoz/szerkesztéshez |

A navigáció a `Shell.Current.GoToAsync(...)` metódust használja. Bármilyen `401 Unauthorized` válasz az API-tól automatikusan átirányít a `//LoginPage` útvonalra és törli a tárolt hitelesítő adatokat.

---

## Külső API Integráció

A Laravel backenddel való összes kommunikáció az **`ApiClient`**-ben van egységbe zárva.

### Alap Konfiguráció

```
Alap URL:    http://localhost:8000   (konfigurálható a MauiProgram.cs fájlban)
Fejlécek:    Accept: application/json
             Authorization: Bearer <token>   (kérésenként beállítva bejelentkezés után)
```

### Használt Végpontok

#### Hitelesítés

| Metódus | Végpont       | Leírás                                                            |
| ------- | ------------- | ----------------------------------------------------------------- |
| `POST`  | `/api/login`  | Felhasználó hitelesítése, token + felhasználói adatok visszaadása |
| `POST`  | `/api/logout` | Szerver oldali token érvénytelenítése                             |
| `GET`   | `/api/user`   | Aktuálisan hitelesített felhasználó lekérése                      |
| `GET`   | `/api/health` | Szerver állapotellenőrzés (2xx-et ad vissza, ha elérhető)         |

#### Felhasználók

| Metódus  | Végpont                             | Leírás                                   |
| -------- | ----------------------------------- | ---------------------------------------- |
| `GET`    | `/api/users?page=N&role=X&search=Y` | Lapozott, szűrt felhasználólista         |
| `GET`    | `/api/users/{id}`                   | Egyetlen felhasználó lekérése ID alapján |
| `POST`   | `/api/users`                        | Új felhasználó létrehozása               |
| `PUT`    | `/api/users/{id}`                   | Meglévő felhasználó frissítése           |
| `DELETE` | `/api/users/{id}`                   | Felhasználó törlése                      |

#### Járművek

| Metódus  | Végpont                               | Leírás                             |
| -------- | ------------------------------------- | ---------------------------------- |
| `GET`    | `/api/cars?page=N&user_id=X&search=Y` | Lapozott, szűrt járműlista         |
| `GET`    | `/api/cars/{id}`                      | Egyetlen jármű lekérése ID alapján |
| `POST`   | `/api/cars`                           | Új jármű létrehozása               |
| `PUT`    | `/api/cars/{id}`                      | Meglévő jármű frissítése           |
| `DELETE` | `/api/cars/{id}`                      | Jármű törlése                      |

#### Hibák

| Metódus  | Végpont                                                | Leírás                                |
| -------- | ------------------------------------------------------ | ------------------------------------- |
| `GET`    | `/api/problems?page=N&category=X&is_active=Y&search=Z` | Lapozott, szűrt hiba-lista            |
| `GET`    | `/api/problems/{id}`                                   | Egyetlen hiba lekérése ID alapján     |
| `POST`   | `/api/problems`                                        | Új hiba létrehozása                   |
| `PUT`    | `/api/problems/{id}`                                   | Meglévő hiba frissítése               |
| `DELETE` | `/api/problems/{id}`                                   | Hiba törlése                          |
| `GET`    | `/api/problems/statistics`                             | Hiba gyakoriság statisztikák lekérése |

#### Szervizjegyek

| Metódus  | Végpont                                                     | Leírás                                        |
| -------- | ----------------------------------------------------------- | --------------------------------------------- |
| `GET`    | `/api/tickets?page=N&status=X&priority=Y&mechanic_id=Z&...` | Lapozott, szűrt jegylista                     |
| `GET`    | `/api/tickets/{id}`                                         | Egyetlen jegy lekérése ID alapján             |
| `POST`   | `/api/tickets`                                              | Új jegy létrehozása                           |
| `PUT`    | `/api/tickets/{id}`                                         | Jegy frissítése (státusz is megváltoztatható) |
| `DELETE` | `/api/tickets/{id}`                                         | Jegy törlése                                  |
| `POST`   | `/api/tickets/{id}/accept`                                  | Jegy elfogadása (→ `assigned` státusz)        |
| `POST`   | `/api/tickets/{id}/start`                                   | Munka megkezdése (→ `in_progress` státusz)    |
| `POST`   | `/api/tickets/{id}/complete`                                | Jegy lezárása befejezettként                  |
| `POST`   | `/api/tickets/{id}/close`                                   | Jegy lezárása (bármikor)                      |
| `GET`    | `/api/tickets/statistics`                                   | Jegy statisztikák lekérése irányítópulthoz    |

### Kérés/Válasz Folyamat

1. Minden hitelesített kérés előtt a `SetAuthHeaderAsync()` beolvassa a tárolt Bearer tokent az `AuthTokenStore`-ból és beállítja a `HttpClient.DefaultRequestHeaders.Authorization` fejlécen.
2. A válaszok `System.Text.Json` használatával kerülnek deszerializálásra `PropertyNameCaseInsensitive = true` beállítással, hogy kezelje a Laravel snake_case JSON mezőit (pl. `created_at`, `last_page`).
3. A nem 2xx válaszok `ApiErrorResponse`-ként kerülnek beolvasásra és `ApiException`-ként dobódnak, amely tartalmazza a HTTP állapotkódot és egy `ValidationErrors` szótárat (a Laravel validációs hibaformátumának megfelelően).

### Hibakezelés

Az `ApiException` három kényelmi jelzőt tesz elérhetővé:

| Tulajdonság         | HTTP Állapotkód |
| ------------------- | --------------- |
| `IsUnauthorized`    | 401             |
| `IsForbidden`       | 403             |
| `IsValidationError` | 422             |

A ViewModel-ek ezeket specifikusan elkapják. A `401` mindig token törlést és bejelentkezésre való átirányítást vált ki.

### `CreateUserAsync` — Válasz Kicsomagolás

A létrehozás végpont visszaadhat egy `{ "data": { ... } }` wrapper-t (Laravel resource formátum) vagy egy nyers felhasználói objektumot. Az `ApiClient` mindkét esetet kezeli `JsonElement` vizsgálattal a deszerializálás előtt.

---

## Hitelesítés

### Bejelentkezési Folyamat

1. A felhasználó megadja az e-mail címet és jelszót a `LoginPage`-en.
2. A `LoginViewModel.LoginAsync()` küld egy `POST /api/login` kérést az `ApiClient`-en keresztül.
3. Sikeres esetben:
   - A visszakapott Bearer token mentésre kerül az `AuthTokenStore.SaveTokenAsync()` segítségével.
   - A felhasználó `id`, `name` és `email` adatai mentésre kerülnek az `AuthTokenStore.SaveUserInfoAsync()` segítségével.
   - A navigáció a `//Main/Dashboard` útvonalra lép tovább.
4. Hiba esetén egy hibaüzenet sáv jelenik meg a felületen (a `HasError` / `ErrorMessage` tulajdonságokhoz kötve).

### Token Perzisztencia

Az `AuthTokenStore` a **`Microsoft.Maui.Storage.Preferences`** szolgáltatást használja (platform-natív kulcs-érték tároló):

| Kulcs             | Érték                                     |
| ----------------- | ----------------------------------------- |
| `auth_token`      | Bearer token karakterlánc                 |
| `auth_user_id`    | Felhasználói azonosító (karakterláncként) |
| `auth_user_name`  | Felhasználó megjelenítési neve            |
| `auth_user_email` | Felhasználó e-mail címe                   |

A `HasTokenAsync()` metódus az indításkor kerül meghívásra annak eldöntésére, hogy átugorható-e a bejelentkezési képernyő.

### Kijelentkezési Folyamat

1. A `DashboardViewModel.LogoutAsync()` meghívja az `ApiClient.LogoutAsync()` metódust, amely a `POST /api/logout` végpontot hívja.
2. Függetlenül attól, hogy az API hívás sikeres-e (a hálózat lehet, hogy nem elérhető), az `AuthTokenStore.Clear()` eltávolítja az összes tárolt hitelesítő adatot, és az `Authorization` fejléc törlésre kerül a `HttpClient`-ből.
3. A navigáció átirányít a `//Login/LoginPage` útvonalra.

---

## Képernyők és Funkciók

### LoginPage (Bejelentkezési Oldal)

- E-mail beviteli mező `Keyboard="Email"` beállítással és törlés gombbal
- Jelszó beviteli mező `IsPassword="True"` beállítással
- Az `Enter` billentyű a jelszó mezőben elindítja a bejelentkezést
- A bejelentkezés gomb letiltásra kerül feldolgozás közben (`InvertBoolConverter` kötéssel)
- A gomb szövege dinamikusan változik: `"Sign In"` → `"Signing in..."` (`BusyTextConverter` segítségével)
- Piros hibasáv (`Border`) csak akkor jelenik meg, ha `HasError = true`
- `ActivityIndicator` látható, amíg `IsBusy = true`

### DashboardPage (Irányítópult)

- Személyre szabott üdvözlő üzenet a tárolt felhasználói adatokból betöltve (pl. `"Welcome, Daniel"`)
- **Szerver állapotjelző**: meghívja a `GET /api/health` végpontot; a pont zöldre/pirosra vált `"Server Online"` / `"Server Offline"` szöveggel (`StatusColorConverter` és `StatusTextConverter` segítségével)
- **Összesített számlálók**: Összes Felhasználó, Jármű, Hiba, Szervizjegy — mind a saját lapozott végpontjukról töltődnek be
- `RefreshView` lehetővé teszi a lehúzással való frissítést az összes statisztika újratöltéséhez
- **Kijelentkezés** gomb az egyedi `Shell.TitleView`-ban
- Navigációs gombok az egyes entitáskezelő képernyőkhöz

### UsersPage (Felhasználók Oldal)

- **Keresősáv** valós idejű `ReturnCommand`-dal, amely a `SearchCommand`-hoz van kötve
- **Szerepkör szűrő Picker** a következő opciókkal: `All`, `user`, `mechanic`, `admin`
- **Lapozott lista** (`CollectionView`), amely felhasználónként nevet, e-mailt és szerepkör jelvényt jelenít meg
- **Szerkesztés** gomb soronként → navigál a `UserDetail?userId={id}` útvonalra
- **Törlés** gomb soronként → megerősítő `DisplayAlert`-ot mutat a `DELETE /api/users/{id}` hívása és az elem `ObservableCollection<User>`-ből való helyi eltávolítása előtt
- **Lapozás vezérlők**: Előző / Következő gombok egy számláló címkével
- **"+ New User"** gomb → navigál a `UserDetail` útvonalra (lekérdezési paraméter nélkül = létrehozás mód)
- Üres állapot nézet, ha nincs találat

### UserDetailPage (Felhasználó Részletek Oldal)

- Kettős mód: **Létrehozás** (cím = `"Create User"`) vagy **Szerkesztés** (cím = `"Edit User"`), amelyet a `userId` lekérdezési paraméter határoz meg
- Mezők: Név, E-mail, Jelszó, Szerepkör (Picker)
- A jelszó mező címkéje az üzemmódtól függően változik (`"Password"` vs `"Password (leave blank to keep current)"`)
- Mentéskor:
  - **Létrehozás**: minden mező kötelező, beleértve a jelszót; `POST /api/users` hívás
  - **Frissítés**: `PUT /api/users/{id}` hívás; a jelszó csak akkor kerül bele a kérésbe, ha a mező nem üres
- Az API validációs hibái (422) a hibasávban jelennek meg
- A Mégse gomb visszalép az előző oldalra (`Shell.Current.GoToAsync("..")`)

### CarsPage (Járművek Oldal)

- **Keresősáv** márkára, modellre, rendszámra és VIN-re keres
- **Lapozott lista** (`CollectionView`), amely járművenként a `DisplayName` (`{Év} {Márka} {Modell}`), rendszám és tulajdonos nevét jeleníti meg
- **Szerkesztés** és **Törlés** gombok soronként (törlés megerősítő párbeszéddel)
- **Lapozás vezérlők** és **"+ New Car"** gomb
- Üres állapot nézet, ha nincs találat

### CarDetailPage (Jármű Részletek Oldal)

- Kettős mód: **Létrehozás** / **Szerkesztés** a `carId` lekérdezési paraméter alapján
- Mezők: Tulajdonos ID, Márka, Modell, Évjárat, Rendszám, VIN (opcionális), Szín (opcionális)
- `POST /api/cars` (létrehozás) vagy `PUT /api/cars/{id}` (frissítés)
- 422-es validációs hibák megjelenítése a hibasávban

### ProblemsPage (Hibák Oldal)

- **Keresősáv** névben és leírásban keres
- **Kategória szűrő Picker**: `All`, `engine`, `transmission`, `electrical`, `brakes`, `suspension`, `steering`, `body`, `other`
- **Lapozott lista** (`CollectionView`), amely hibánként nevet, kategóriát és aktív/inaktív jelvényt jelenít meg
- **Szerkesztés** és **Törlés** gombok soronként
- **Lapozás vezérlők** és **"+ New Problem"** gomb

### ProblemDetailPage (Hiba Részletek Oldal)

- Kettős mód: **Létrehozás** / **Szerkesztés** a `problemId` lekérdezési paraméter alapján
- Mezők: Név, Kategória (Picker), Leírás (opcionális), Aktív (Toggle/Switch)
- `POST /api/problems` (létrehozás) vagy `PUT /api/problems/{id}` (frissítés)

### TicketsPage (Szervizjegyek Oldal)

- **Státusz szűrő Picker**: `All`, `open`, `assigned`, `in_progress`, `completed`, `closed`
- **Prioritás szűrő Picker**: `All`, `low`, `medium`, `high`, `urgent`
- **Lapozott lista** (`CollectionView`), amely jegyenként az azonosítót, leírást, státuszt, prioritást, tulajdonost és járművet jeleníti meg
- **Szerkesztés** és **Törlés** gombok soronként
- **Lapozás vezérlők** és **"+ New Ticket"** gomb

### TicketDetailPage (Szervizjegy Részletek Oldal)

- Kettős mód: **Létrehozás** / **Szerkesztés** a `ticketId` lekérdezési paraméter alapján
- Mezők: Jármű ID, Leírás, Prioritás (Picker), Státusz (Picker szerkesztés módban)
- **Munkafolyamat gombok** szerkesztés módban (az aktuális státuszra érzékenyek):
  - **Accept** — megjelenik, ha `CanAccept` igaz (`open` státusz)
  - **Start** — megjelenik, ha `CanStart` igaz (`assigned` státusz)
  - **Complete** — megjelenik, ha `CanComplete` igaz (`in_progress` státusz)
  - **Close** — megjelenik, ha `CanClose` igaz (nem `closed` és nem `completed`)
- `POST /api/tickets` (létrehozás) vagy `PUT /api/tickets/{id}` (frissítés)
- Státuszváltó végpontok: `POST /api/tickets/{id}/accept|start|complete|close`

---

## Adatmodellek

### `User`

| Tulajdonság   | JSON Kulcs    | Típus                                            |
| ------------- | ------------- | ------------------------------------------------ |
| `Id`          | `id`          | `int`                                            |
| `Name`        | `name`        | `string`                                         |
| `Email`       | `email`       | `string`                                         |
| `CreatedAt`   | `created_at`  | `DateTime?`                                      |
| `UpdatedAt`   | `updated_at`  | `DateTime?`                                      |
| `Roles`       | `roles`       | `List<Role>`                                     |
| `RoleDisplay` | _(számított)_ | `string` — vesszővel elválasztott szerepkörnevek |

### `Role`

| Tulajdonság | JSON Kulcs | Típus    |
| ----------- | ---------- | -------- |
| `Id`        | `id`       | `int`    |
| `Name`      | `name`     | `string` |

### `Car`

| Tulajdonság    | JSON Kulcs      | Típus                                         |
| -------------- | --------------- | --------------------------------------------- |
| `Id`           | `id`            | `int`                                         |
| `UserId`       | `user_id`       | `int`                                         |
| `Make`         | `make`          | `string`                                      |
| `Model`        | `model`         | `string`                                      |
| `Year`         | `year`          | `int`                                         |
| `LicensePlate` | `license_plate` | `string`                                      |
| `Vin`          | `vin`           | `string?`                                     |
| `Color`        | `color`         | `string?`                                     |
| `CreatedAt`    | `created_at`    | `DateTime?`                                   |
| `UpdatedAt`    | `updated_at`    | `DateTime?`                                   |
| `User`         | `user`          | `User?`                                       |
| `DisplayName`  | _(számított)_   | `string` — `"{Year} {Make} {Model}"` formátum |

### `Problem`

| Tulajdonság     | JSON Kulcs    | Típus                     |
| --------------- | ------------- | ------------------------- |
| `Id`            | `id`          | `int`                     |
| `Name`          | `name`        | `string`                  |
| `Category`      | `category`    | `string`                  |
| `Description`   | `description` | `string?`                 |
| `IsActive`      | `is_active`   | `bool`                    |
| `CreatedAt`     | `created_at`  | `DateTime?`               |
| `UpdatedAt`     | `updated_at`  | `DateTime?`               |
| `Pivot`         | `pivot`       | `ProblemPivot?`           |
| `ActiveDisplay` | _(számított)_ | `"Active"` / `"Inactive"` |

Érvényes kategóriák: `engine`, `transmission`, `electrical`, `brakes`, `suspension`, `steering`, `body`, `other`

### `ProblemPivot`

Pivot adat, amely akkor jelenik meg, ha egy hiba egy jegyen keresztül töltődik be.

| Tulajdonság | JSON Kulcs   | Típus     |
| ----------- | ------------ | --------- |
| `TicketId`  | `ticket_id`  | `int`     |
| `ProblemId` | `problem_id` | `int`     |
| `Notes`     | `notes`      | `string?` |

### `ProblemStatistics`

| Tulajdonság           | JSON Kulcs              | Típus                    |
| --------------------- | ----------------------- | ------------------------ |
| `TotalProblems`       | `total_problems`        | `int`                    |
| `ActiveProblems`      | `active_problems`       | `int`                    |
| `ProblemsByFrequency` | `problems_by_frequency` | `List<ProblemFrequency>` |

### `Ticket`

| Tulajdonság       | JSON Kulcs     | Típus                                                |
| ----------------- | -------------- | ---------------------------------------------------- |
| `Id`              | `id`           | `int`                                                |
| `UserId`          | `user_id`      | `int`                                                |
| `MechanicId`      | `mechanic_id`  | `int?`                                               |
| `CarId`           | `car_id`       | `int`                                                |
| `Status`          | `status`       | `string`                                             |
| `Priority`        | `priority`     | `string`                                             |
| `Description`     | `description`  | `string`                                             |
| `AcceptedAt`      | `accepted_at`  | `DateTime?`                                          |
| `CompletedAt`     | `completed_at` | `DateTime?`                                          |
| `CreatedAt`       | `created_at`   | `DateTime?`                                          |
| `UpdatedAt`       | `updated_at`   | `DateTime?`                                          |
| `User`            | `user`         | `User?`                                              |
| `Mechanic`        | `mechanic`     | `User?`                                              |
| `Car`             | `car`          | `Car?`                                               |
| `Problems`        | `problems`     | `List<Problem>?`                                     |
| `StatusDisplay`   | _(számított)_  | Lokalizált státusz szöveg                            |
| `PriorityDisplay` | _(számított)_  | Lokalizált prioritás szöveg                          |
| `OwnerDisplay`    | _(számított)_  | Tulajdonos neve vagy `"User #{id}"`                  |
| `MechanicDisplay` | _(számított)_  | Szerelő neve, `"Mechanic #{id}"` vagy `"Unassigned"` |
| `CarDisplay`      | _(számított)_  | `Car.DisplayName` vagy `"Car #{id}"`                 |
| `CanAccept`       | _(számított)_  | `true` ha státusz `open`                             |
| `CanStart`        | _(számított)_  | `true` ha státusz `assigned`                         |
| `CanComplete`     | _(számított)_  | `true` ha státusz `in_progress`                      |
| `CanClose`        | _(számított)_  | `true` ha státusz nem `closed` és nem `completed`    |

Érvényes státuszok: `open` → `assigned` → `in_progress` → `completed` / `closed`  
Érvényes prioritások: `low`, `medium`, `high`, `urgent`

### `TicketStatistics`

| Tulajdonság         | JSON Kulcs            | Típus                  |
| ------------------- | --------------------- | ---------------------- |
| `TotalTickets`      | `total_tickets`       | `int`                  |
| `ByStatus`          | `by_status`           | `TicketStatusCounts`   |
| `ByPriority`        | `by_priority`         | `TicketPriorityCounts` |
| `OpenTickets`       | `open_tickets`        | `int`                  |
| `AssignedTickets`   | `assigned_tickets`    | `int`                  |
| `InProgressTickets` | `in_progress_tickets` | `int`                  |
| `CompletedToday`    | `completed_today`     | `int`                  |

### `PaginatedResponse<T>`

Megfelel a Laravel alapértelmezett lapozó JSON struktúrájának:

| Tulajdonság   | JSON Kulcs     |
| ------------- | -------------- |
| `Data`        | `data`         |
| `CurrentPage` | `current_page` |
| `LastPage`    | `last_page`    |
| `PerPage`     | `per_page`     |
| `Total`       | `total`        |

### `LoginRequest` / `LoginResponse`

A `LoginRequest` az `email` és `password` mezőket képezi le. A `LoginResponse` a `message`, `token` (Bearer karakterlánc) és egy beágyazott `user` objektumot képez le.

### `ApiErrorResponse`

A `message` (karakterlánc) és `errors` (`Dictionary<string, List<string>>`) mezőket képezi le, a Laravel validációs hibaformátumának megfelelően.

---

## Érték Konverterek

A `Converters/Converters.cs` fájlban vannak definiálva és statikus erőforrásként regisztrálva az `App.xaml`-ben.

| Konverter              | Bemenet                             | Kimenet  | Felhasználás                           |
| ---------------------- | ----------------------------------- | -------- | -------------------------------------- |
| `InvertBoolConverter`  | `bool`                              | `bool`   | Gomb letiltása feldolgozás közben      |
| `BusyTextConverter`    | `bool` + `"TextA\|TextB"` paraméter | `string` | Dinamikus gomb szöveg / címke szöveg   |
| `StatusColorConverter` | `bool` (`IsServerOnline`)           | `Color`  | Zöld / Piros pont az irányítópulton    |
| `StatusTextConverter`  | `bool` (`IsServerOnline`)           | `string` | `"Server Online"` / `"Server Offline"` |

---

## Téma és Stílus

Az alkalmazás **sötét témát** használ lila elsődleges színnel. Az összes szín statikus erőforrásként van definiálva a `Resources/Styles/Colors.xaml` fájlban:

| Kulcs          | Hex       | Szerep                                     |
| -------------- | --------- | ------------------------------------------ |
| `Primary`      | `#512BD4` | Gombok, kiemelések, aktív szerepkör címkék |
| `Gray950`      | `#141414` | Oldal háttér (bejelentkezés)               |
| `Gray900`      | `#212121` | Kártya / keret hátterek                    |
| `Gray600`      | `#404040` | Másodlagos gombok                          |
| `Gray400`      | `#919191` | Halvány szöveg (e-mail, alcímek)           |
| `Gray300`      | `#ACACAC` | Űrlap címkék                               |
| `Gray200`      | `#C8C8C8` | Beviteli mező címkék                       |
| `Red100Accent` | `#FF5252` | Hibasávok, törlés gombok                   |

A XAML fordítási időben kerül lefordításra a **`MauiXamlInflator=SourceGen`** beállítással, amely C# kódot generál a XAML-ből (ahogy a `Views_LoginPage.xaml.xsg.cs` fájlban látható), javítva az indítási teljesítményt és lehetővé téve az AOT kompatibilitást.

---

## Tesztelés

A megoldás tartalmaz egy különálló **`Admin.Tests`** xUnit tesztprojektet (`.NET 10`), amely az `Admin` főprojektre hivatkozik. A tesztek **Moq**-ot használnak a függőségek izolálásához.

### Tesztkategóriák

#### Modell tesztek (`Admin.Tests/Models/`)

A tesztek ellenőrzik a modell deszializációját, a számított tulajdonságokat és az alapértelmezett értékeket.

| Tesztfájl                   | Amit tesztel                                                                                                         |
| --------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `CarModelTests.cs`          | `DisplayName` formátum, alapértelmezett értékek, snake_case JSON deszializáció opcionális mezőkkel                   |
| `CarRequestsTests.cs`       | `CreateCarRequest` és `UpdateCarRequest` JSON szerializáció és mezők                                                 |
| `ProblemModelTests.cs`      | `ActiveDisplay` logika, `ProblemPivot` deszializáció, snake_case JSON leképezés                                      |
| `ProblemRequestsTests.cs`   | `CreateProblemRequest` és `UpdateProblemRequest` JSON szerializáció                                                  |
| `ProblemStatisticsTests.cs` | `ProblemStatistics` és `ProblemFrequency` deszializáció                                                              |
| `TicketModelTests.cs`       | Munkafolyamat jelzők (`CanAccept`, `CanStart`, `CanComplete`, `CanClose`), display tulajdonságok, JSON deszializáció |
| `TicketRequestsTests.cs`    | `CreateTicketRequest` és `UpdateTicketRequest` JSON szerializáció (problem_ids-szel együtt)                          |
| `TicketStatisticsTests.cs`  | `TicketStatistics`, `TicketStatusCounts`, `TicketPriorityCounts` deszializáció                                       |

#### Szolgáltatás tesztek (`Admin.Tests/Services/`)

| Tesztfájl              | Amit tesztel                                                                      |
| ---------------------- | --------------------------------------------------------------------------------- |
| `ApiExceptionTests.cs` | `IsUnauthorized`, `IsForbidden`, `IsValidationError` jelzők; üzenet és hibaszótár |

#### ViewModel / API kliens tesztek (`Admin.Tests/ViewModels/`)

Ezek a tesztek az `IApiClient` interfészt Moq segítségével izolálják, és az `ApiClient` ViewModel-eken átmenő hívásait ellenőrzik.

| Tesztfájl                   | Amit tesztel                                                                                                                                                                                                       |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `CarsApiClientTests.cs`     | `GetCarsAsync`, `GetCarAsync`, `CreateCarAsync`, `UpdateCarAsync`, `DeleteCarAsync`                                                                                                                                |
| `ProblemsApiClientTests.cs` | `GetProblemsAsync`, `GetProblemAsync`, `CreateProblemAsync`, `UpdateProblemAsync`, `DeleteProblemAsync`, `GetProblemStatisticsAsync`                                                                               |
| `TicketsApiClientTests.cs`  | `GetTicketsAsync`, `GetTicketAsync`, `CreateTicketAsync`, `UpdateTicketAsync`, `DeleteTicketAsync`, `AcceptTicketAsync`, `StartTicketAsync`, `CompleteTicketAsync`, `CloseTicketAsync`, `GetTicketStatisticsAsync` |

### Tesztek futtatása

```bash
cd /Users/dluhidaniel/Admin
dotnet test Admin.Tests/Admin.Tests.csproj
```

---

## Elért Eredmények

| Funkció                                                                          | Állapot |
| -------------------------------------------------------------------------------- | ------- |
| Platformfüggetlen MAUI projekt (Android, iOS, macOS, Windows)                    | ✅ Kész |
| DI konténer bekötés (szolgáltatások, viewmodel-ek, oldalak)                      | ✅ Kész |
| Állandó munkamenet (automatikus bejelentkezés újraindításkor, ha van token)      | ✅ Kész |
| Bejelentkezés Laravel Bearer token hitelesítéssel                                | ✅ Kész |
| Kijelentkezés szerver oldali token érvénytelenítéssel                            | ✅ Kész |
| Automatikus átirányítás bejelentkezésre 401-es válaszoknál                       | ✅ Kész |
| Flyout oldalsáv navigáció (Dashboard, Users, Cars, Problems, Tickets)            | ✅ Kész |
| Irányítópult szerver állapotellenőrzéssel (`/api/health`)                        | ✅ Kész |
| Irányítópult összesített statisztikákkal (felhasználók, járművek, hibák, jegyek) | ✅ Kész |
| Lehúzással való frissítés az irányítópulton                                      | ✅ Kész |
| Lapozott felhasználólista szerver-vezérelt lapozással                            | ✅ Kész |
| Felhasználó keresés és szerepkör szűrés                                          | ✅ Kész |
| Felhasználó CRUD (létrehozás, szerkesztés, törlés megerősítéssel)                | ✅ Kész |
| Lapozott járműlista keresővel                                                    | ✅ Kész |
| Jármű CRUD (létrehozás, szerkesztés, törlés megerősítéssel)                      | ✅ Kész |
| Lapozott hiba-lista keresővel és kategória szűrővel                              | ✅ Kész |
| Hiba CRUD (létrehozás, szerkesztés, törlés megerősítéssel)                       | ✅ Kész |
| Lapozott szervizjegy-lista státusz és prioritás szűrőkkel                        | ✅ Kész |
| Szervizjegy CRUD (létrehozás, szerkesztés, törlés megerősítéssel)                | ✅ Kész |
| Jegy munkafolyamat műveletek (Accept / Start / Complete / Close)                 | ✅ Kész |
| Státusz-érzékeny munkafolyamat gombok (`CanAccept`, `CanStart`, stb.)            | ✅ Kész |
| Űrlap validáció szerver hibaüzenetek megjelenítésével (422-es válaszok)          | ✅ Kész |
| Feldolgozási állapot kezelés az összes képernyőn                                 | ✅ Kész |
| xUnit tesztprojekt modell, szolgáltatás és API kliens tesztekkel                 | ✅ Kész |
| XAML forrásgenerált fordítás                                                     | ✅ Kész |
| Sötét téma egységes színpalettával                                               | ✅ Kész |

### Függőben Lévő / Ismert Teendők

- A **Backend URL** fixen `http://localhost:8000`-re van kódolva a `MauiProgram.cs` fájlban — éles telepítéshez környezeti/konfigurációs fájlba kellene áthelyezni.
- Nincs refresh token mechanizmus — ha a Bearer token lejár a szerver oldalon, a felhasználó egyszerűen átirányításra kerül a bejelentkezéshez.
- A `GetCurrentUserAsync()` metódus (`GET /api/user`) definiálva van az `ApiClient`-ben, de jelenleg egyetlen ViewModel sem hívja meg (a felhasználói adatok helyette a tárolt `Preferences`-ből töltődnek be az irányítópulton).
- A ViewModel-ek (`UsersViewModel`, `CarsViewModel`, stb.) nincsenek egységtesztelve — ezek MAUI-specifikus függőségek (`Shell`, `DisplayAlert`) miatt csak integrációs- vagy UI-tesztekkel tesztelhetők teljeskörűen.
- A hiba-statisztikák (`/api/problems/statistics`) és jegy-statisztikák (`/api/tickets/statistics`) lekérve vannak, de az irányítópulton részletes bontásban nem jelennek meg — csak az összesített számok láthatók.
