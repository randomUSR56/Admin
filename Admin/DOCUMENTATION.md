# OnlyFix Admin — Műszaki Dokumentáció

## Összefoglaló

Az **OnlyFix Admin** egy platformfüggetlen mobil/asztali adminisztrációs panel, amely **.NET 10 MAUI** keretrendszerrel készült. Egy **Laravel REST API backendhez** csatlakozik (amelyet a `http://localhost:8000` címen vár), és lehetővé teszi a hitelesített adminisztrátorok számára az OnlyFix platform felhasználóinak kezelését. Az alkalmazás jelenleg támogatja a Bearer token alapú bejelentkezést, egy irányítópultot élő szerver-állapotellenőrzéssel, valamint teljes CRUD (Létrehozás, Olvasás, Frissítés, Törlés) műveleteket a felhasználókon szerepkörkezeléssel és lapozott kereséssel.

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
11. [Elért Eredmények](#elért-eredmények)

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
Admin/
├── App.xaml / App.xaml.cs          # Alkalmazás belépési pont, indítási útválasztás
├── AppShell.xaml / AppShell.xaml.cs # Shell navigációs struktúra
├── MauiProgram.cs                   # DI konténer beállítás
│
├── Models/
│   ├── AuthModels.cs               # LoginRequest, LoginResponse, ApiErrorResponse
│   ├── User.cs                     # User és Role modellek
│   ├── UserRequests.cs             # CreateUserRequest, UpdateUserRequest
│   └── PaginatedResponse.cs        # Általános lapozott API wrapper
│
├── Services/
│   ├── ApiClient.cs                # Összes HTTP hívás a Laravel backendhez
│   └── AuthTokenStore.cs           # Auth token és felhasználói adatok mentése/lekérése
│
├── ViewModels/
│   ├── LoginViewModel.cs           # Bejelentkezési űrlap logika
│   ├── DashboardViewModel.cs       # Irányítópult statisztikák és navigáció
│   ├── UsersViewModel.cs           # Felhasználólista, keresés, szűrés, lapozás, törlés
│   └── UserDetailViewModel.cs      # Felhasználó létrehozása / szerkesztése
│
├── Views/
│   ├── LoginPage.xaml              # Bejelentkezési űrlap
│   ├── DashboardPage.xaml          # Áttekintés + szerver állapot
│   ├── UsersPage.xaml              # Lapozott felhasználólista
│   └── UserDetailPage.xaml         # Felhasználó létrehozó / szerkesztő űrlap
│
├── Converters/
│   └── Converters.cs              # InvertBoolConverter, BusyTextConverter,
│                                  # StatusColorConverter, StatusTextConverter
│
└── Resources/
    └── Styles/
        └── Colors.xaml            # Globális színpaletta
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

- **Token megtalálva** → navigál a `//Main/Dashboard` útvonalra (átugorja a bejelentkezést)
- **Nincs token** → navigál a `//Login/LoginPage` útvonalra

### `AppShell.xaml`

Két `TabBar` szekciót definiál:

| Útvonal             | Leírás                                                                       |
| ------------------- | ---------------------------------------------------------------------------- |
| `//Login/LoginPage` | Bejelentkezési képernyő (nincs navigációs sáv, nincs tab sáv)                |
| `//Main/Dashboard`  | Irányítópult fül                                                             |
| `//Main/Users`      | Felhasználók fül                                                             |
| `UserDetail`        | Shell útvonalként regisztrálva (push navigáció) létrehozáshoz/szerkesztéshez |

A szekciók közötti navigáció a `Shell.Current.GoToAsync(...)` metódust használja. Bármilyen `401 Unauthorized` válasz az API-tól automatikusan átirányít a `//Login/LoginPage` útvonalra és törli a tárolt hitelesítő adatokat.

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

| Metódus  | Végpont                             | Leírás                                                            |
| -------- | ----------------------------------- | ----------------------------------------------------------------- |
| `POST`   | `/api/login`                        | Felhasználó hitelesítése, token + felhasználói adatok visszaadása |
| `POST`   | `/api/logout`                       | Szerver oldali token érvénytelenítése                             |
| `GET`    | `/api/user`                         | Aktuálisan hitelesített felhasználó lekérése                      |
| `GET`    | `/api/users?page=N&role=X&search=Y` | Lapozott, szűrt felhasználólista                                  |
| `GET`    | `/api/users/{id}`                   | Egyetlen felhasználó lekérése ID alapján                          |
| `POST`   | `/api/users`                        | Új felhasználó létrehozása                                        |
| `PUT`    | `/api/users/{id}`                   | Meglévő felhasználó frissítése                                    |
| `DELETE` | `/api/users/{id}`                   | Felhasználó törlése                                               |
| `GET`    | `/api/health`                       | Szerver állapotellenőrzés (2xx-et ad vissza, ha elérhető)         |

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
- **Összes Felhasználó** szám betöltése a `GET /api/users` végpontról
- `RefreshView` lehetővé teszi a lehúzással való frissítést az összes statisztika újratöltéséhez
- **Kijelentkezés** gomb az egyedi `Shell.TitleView`-ban
- Navigációs gomb a Felhasználók képernyőre

### UsersPage (Felhasználók Oldal)

- **Keresősáv** valós idejű `ReturnCommand`-dal, amely a `SearchCommand`-hoz van kötve
- **Szerepkör szűrő Picker** a következő opciókkal: `All`, `user`, `mechanic`, `admin`
- **Lapozott lista** (`CollectionView`), amely felhasználónként nevet, e-mailt és szerepkör jelvényt jelenít meg
- **Szerkesztés** gomb soronként → navigál a `UserDetail?userId={id}` útvonalra
- **Törlés** gomb soronként → megerősítő `DisplayAlert`-ot mutat a `DELETE /api/users/{id}` hívása és az elem `ObservableCollection<User>`-ből való helyi eltávolítása előtt
- **Lapozás vezérlők**: Előző / Következő gombok egy címkével, amely mutatja: `Page X of Y (Z total users)`
- **"+ New User"** gomb → navigál a `UserDetail` útvonalra (lekérdezési paraméter nélkül = létrehozás mód)
- Üres állapot nézet, ha nincs találat

### UserDetailPage (Felhasználó Részletek Oldal)

- Kettős mód: **Létrehozás** (cím = `"Create User"`) vagy **Szerkesztés** (cím = `"Edit User"`), amelyet a `userId` lekérdezési paraméter határoz meg
- Mezők: Név, E-mail, Jelszó, Szerepkör (Picker)
- A jelszó mező címkéje az üzemmódtól függően változik (`"Password"` vs `"Password (leave blank to keep current)"`) a `BusyTextConverter` segítségével, ahol az `IsExistingUser` a kötési érték
- Mentéskor:
  - **Létrehozás**: minden mező kötelező, beleértve a jelszót; `POST /api/users` hívás
  - **Frissítés**: `PUT /api/users/{id}` hívás; a jelszó csak akkor kerül bele a kérésbe, ha a mező nem üres
- Az API validációs hibái (422) a hibasávban jelennek meg, az összes üzenet újsorral összefűzve
- A Mégse gomb visszalép az előző oldalra (`Shell.Current.GoToAsync("..")`)

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

A backend több-a-többhöz szerepkör kapcsolatot használ (Spatie Laravel Permission vagy azzal egyenértékű). Az alkalmazás csak az első szerepkört olvassa ki megjelenítéshez, de egyetlen `role` karakterláncot küld létrehozáskor/frissítéskor.

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

## Elért Eredmények

| Funkció                                                                     | Állapot |
| --------------------------------------------------------------------------- | ------- |
| Platformfüggetlen MAUI projekt (Android, iOS, macOS, Windows)               | ✅ Kész |
| DI konténer bekötés (szolgáltatások, viewmodel-ek, oldalak)                 | ✅ Kész |
| Állandó munkamenet (automatikus bejelentkezés újraindításkor, ha van token) | ✅ Kész |
| Bejelentkezés Laravel Bearer token hitelesítéssel                           | ✅ Kész |
| Kijelentkezés szerver oldali token érvénytelenítéssel                       | ✅ Kész |
| Automatikus átirányítás bejelentkezésre 401-es válaszoknál                  | ✅ Kész |
| Irányítópult szerver állapotellenőrzéssel (`/api/health`)                   | ✅ Kész |
| Irányítópult összesített felhasználószámmal                                 | ✅ Kész |
| Lehúzással való frissítés az irányítópulton                                 | ✅ Kész |
| Lapozott felhasználólista szerver-vezérelt lapozással                       | ✅ Kész |
| Felhasználó keresés kulcsszó alapján                                        | ✅ Kész |
| Felhasználó szűrés szerepkör alapján                                        | ✅ Kész |
| Új felhasználó létrehozása (szerepkör hozzárendeléssel)                     | ✅ Kész |
| Meglévő felhasználó szerkesztése (név, e-mail, jelszó, szerepkör)           | ✅ Kész |
| Felhasználó törlése megerősítő párbeszédablakkal                            | ✅ Kész |
| Űrlap validáció szerver hibaüzenetek megjelenítésével (422-es válaszok)     | ✅ Kész |
| Feldolgozási állapot kezelés az összes képernyőn                            | ✅ Kész |
| XAML forrásgenerált fordítás                                                | ✅ Kész |
| Sötét téma egységes színpalettával                                          | ✅ Kész |

### Függőben Lévő / Ismert Teendők

- A **Backend URL** fixen `http://localhost:8000`-re van kódolva a `MauiProgram.cs` fájlban — éles telepítéshez környezeti/konfigurációs fájlba kellene áthelyezni.
- Nincsenek egység- vagy integrációs tesztek a megoldásban.
- Nincs refresh token mechanizmus — ha a Bearer token lejár a szerver oldalon, a felhasználó egyszerűen átirányításra kerül a bejelentkezéshez.
- A `GetCurrentUserAsync()` metódus (`GET /api/user`) definiálva van az `ApiClient`-ben, de jelenleg egyetlen ViewModel sem hívja meg (a felhasználói adatok helyette a tárolt `Preferences`-ből töltődnek be az irányítópulton).
