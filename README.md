# OnlyFix Admin

Az **OnlyFix Admin** egy platformfüggetlen adminisztrációs alkalmazás, amely az [OnlyFix](https://github.com/randomUSR56/onlyfix) rendszer adminisztrátori felülete. Segítségével a jogosult adminisztrátorok kezelhetik a felhasználókat, járműveket, hibabejelentéseket és szervizjegyeket – mindez egyetlen, átlátható felületen keresztül.

---

## Miről szól ez az alkalmazás?

Az OnlyFix egy jármű-szervizelési platform, ahol a felhasználók hibabejelentéseket és szervizjegyeket hozhatnak létre a járműveikkel kapcsolatban. Ez az **Admin** alkalmazás a rendszer adminisztrátori panelje: lehetővé teszi az összes adat (felhasználók, járművek, hibák, jegyek) megtekintését, létrehozását, szerkesztését és törlését, valamint valós idejű statisztikákat és szerver-állapotellenőrzést kínál.

Az alkalmazás **.NET 10 MAUI** keretrendszerrel készült, így Android, iOS, macOS és Windows platformokon egyaránt futtatható.

---

## Előfeltételek

Az alkalmazás futtatásához az alábbiak szükségesek:

- **.NET 10 SDK** telepítve ([letöltés](https://dotnet.microsoft.com/download/dotnet/10.0))
- **.NET MAUI** munkaterhelés telepítve:
  ```bash
  dotnet workload install maui
  ```
- Futó **OnlyFix Laravel backend** (alapértelmezés szerint `http://onlyfix.local` címen várja az alkalmazás)
- A célplatformhoz szükséges fejlesztői eszközök (pl. Android SDK Android esetén, Xcode iOS/macOS esetén)

---

## Beállítás és futtatás

### 1. Repository klónozása

```bash
git clone https://github.com/randomUSR56/Admin.git
cd Admin
```

### 2. Backend URL konfigurálása

Nyisd meg az `Admin/MauiProgram.cs` fájlt, és állítsd be a Laravel backend címét:

```csharp
client.BaseAddress = new Uri("http://onlyfix.local");
```

Cseréld le a `http://onlyfix.local` értéket a saját szervered URL-jére (pl. `http://localhost:8000`).

### 3. Alkalmazás futtatása

Az alkalmazást az alábbi paranccsal indíthatod el (a kívánt platformot megadva):

```bash
# Android
dotnet build -t:Run -f net10.0-android

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0

# macOS
dotnet build -t:Run -f net10.0-maccatalyst
```

Vagy nyisd meg az `Admin.slnx` megoldásfájlt Visual Studioban, válaszd ki a célplatformot, majd indítsd el az alkalmazást.

---

## Bejelentkezés

Az alkalmazás indításakor, ha még nincs mentett bejelentkezési token, a **Bejelentkezés** képernyő jelenik meg. Írd be az adminisztrátor e-mail-címedet és jelszavadat, majd nyomj a bejelentkezés gombra. Sikeres hitelesítés után az alkalmazás elmenti a Bearer tokent, és automatikusan az irányítópultra navigál. Legközelebb az alkalmazás indításakor – ha a token még érvényes – a bejelentkezési képernyő ki lesz hagyva.

---

## Főbb funkciók

### Irányítópult
Az irányítópult megnyitásakor az alkalmazás ellenőrzi a backend elérhetőségét, és összesített statisztikákat jelenít meg a hibákról és szervizjegyekről (pl. státusz szerinti bontás, prioritás szerinti bontás).

### Felhasználók kezelése
A felhasználók listája lapozható és kereshető. Lehetőség van szerepkör szerinti szűrésre is. Minden felhasználóhoz megtekinthető a részletes adatlap, ahol az adatok szerkeszthetők, vagy a felhasználó törölhető. Új felhasználó is létrehozható közvetlenül az alkalmazásból.

### Járművek kezelése
Az összes regisztrált jármű listázható, kereshető és szűrhető. A jármű részletes adatlapján szerkesztés és törlés is elvégezhető, illetve új jármű vehető fel a rendszerbe.

### Hibák kezelése
A hibabejelentések lapozható listában jelennek meg, kategória és aktív/inaktív státusz szerint szűrhetők. Az egyes hibák szerkeszthetők, törölhetők, vagy újak hozhatók létre.

### Szervizjegyek kezelése
A szervizjegyek kezelése a legteljesebb funkcióval rendelkező modul. A jegyek státusz (pl. nyitott, folyamatban, lezárt) és prioritás szerint szűrhetők. Az egyes jegyek részletes adatlapján munkafolyamat-műveletek is elérhetők: jegy elfogadása, munka megkezdése, befejezése vagy lezárása.

---

## Tesztek futtatása

A megoldáshoz xUnit-alapú tesztprojekt (`Admin.Tests`) is tartozik. A tesztek futtatásához:

```bash
dotnet test
```

---

## Mélyebb megismerés

Ha részletesebben szeretnél megismerkedni az alkalmazás architektúrájával, az API végpontokkal, az adatmodellekkel és a teszteléssel, olvasd el a műszaki dokumentációt:

👉 [DOCUMENTATION.md](Admin/DOCUMENTATION.md)