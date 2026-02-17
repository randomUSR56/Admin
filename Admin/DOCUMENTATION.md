# OnlyFix Admin — Technical Documentation

## Summary

**OnlyFix Admin** is a cross-platform mobile/desktop administration panel built with **.NET 10 MAUI**. It connects to a **Laravel REST API backend** (expected at `http://localhost:8000`) and allows authenticated administrators to manage the users of the OnlyFix platform. The app currently supports login with Bearer token authentication, a dashboard with a live server health check, and full CRUD (Create, Read, Update, Delete) operations on users with role management and paginated search.

---

## Table of Contents

1. [Technology Stack](#technology-stack)
2. [Project Structure](#project-structure)
3. [Architecture Overview](#architecture-overview)
4. [Application Startup & Navigation](#application-startup--navigation)
5. [Third-Party API Integration](#third-party-api-integration)
6. [Authentication](#authentication)
7. [Screens & Features](#screens--features)
8. [Data Models](#data-models)
9. [Value Converters](#value-converters)
10. [Theming & Styling](#theming--styling)
11. [What Has Been Achieved](#what-has-been-achieved)

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 MAUI (Multi-platform App UI) |
| UI Pattern | MVVM with `CommunityToolkit.Mvvm` |
| HTTP Client | `System.Net.Http.HttpClient` (typed, DI-managed) |
| Serialization | `System.Text.Json` |
| Token Storage | `Microsoft.Maui.Storage.Preferences` |
| XAML Compilation | Source Generator (`MauiXamlInflator=SourceGen`) |
| Target Platforms | Android, iOS, macOS Catalyst, Windows |
| Backend | Laravel PHP REST API (Bearer token via Sanctum/Passport) |

---

## Project Structure

```
Admin/
├── App.xaml / App.xaml.cs          # Application entry point, startup routing
├── AppShell.xaml / AppShell.xaml.cs # Shell navigation structure
├── MauiProgram.cs                   # DI container setup
│
├── Models/
│   ├── AuthModels.cs               # LoginRequest, LoginResponse, ApiErrorResponse
│   ├── User.cs                     # User and Role models
│   ├── UserRequests.cs             # CreateUserRequest, UpdateUserRequest
│   └── PaginatedResponse.cs        # Generic paginated API wrapper
│
├── Services/
│   ├── ApiClient.cs                # All HTTP calls to the Laravel backend
│   └── AuthTokenStore.cs           # Persists/retrieves auth token and user info
│
├── ViewModels/
│   ├── LoginViewModel.cs           # Login form logic
│   ├── DashboardViewModel.cs       # Dashboard stats and navigation
│   ├── UsersViewModel.cs           # User list, search, filter, pagination, delete
│   └── UserDetailViewModel.cs      # Create / edit a single user
│
├── Views/
│   ├── LoginPage.xaml              # Sign-in form
│   ├── DashboardPage.xaml          # Overview + server status
│   ├── UsersPage.xaml              # Paginated user list
│   └── UserDetailPage.xaml         # Create / edit user form
│
├── Converters/
│   └── Converters.cs              # InvertBoolConverter, BusyTextConverter,
│                                  # StatusColorConverter, StatusTextConverter
│
└── Resources/
    └── Styles/
        └── Colors.xaml            # Global color palette
```

---

## Architecture Overview

The app follows a strict **MVVM** pattern:

- **Views** are pure XAML with compiled bindings (`x:DataType`). They contain no business logic.
- **ViewModels** hold all state (via `[ObservableProperty]`) and all commands (via `[RelayCommand]`), sourced from `CommunityToolkit.Mvvm`. They communicate with services but never directly reference views.
- **Services** (`ApiClient`, `AuthTokenStore`) are injected via the built-in .NET MAUI DI container registered in `MauiProgram.cs`.

```
View  <──bindings──>  ViewModel  <──DI──>  ApiClient  <──HTTP──>  Laravel API
                                    └──DI──>  AuthTokenStore
```

ViewModels are registered as **Transient** (new instance per navigation), while `AuthTokenStore` is **Singleton** (shared across the app lifetime). `ApiClient` is registered via `AddHttpClient<T>`, which creates a managed `HttpClient` with a preconfigured base address and `Accept: application/json` header.

---

## Application Startup & Navigation

### `MauiProgram.cs`
Bootstraps the DI container. Registers:
- `AuthTokenStore` as singleton
- `ApiClient` as a typed `HttpClient` with base URL `http://localhost:8000`
- All ViewModels as transient
- All Pages as transient

### `App.xaml.cs`
On `Window.Loaded`, checks whether a token exists in `AuthTokenStore`:
- **Token found** → navigates to `//Main/Dashboard` (skips login)
- **No token** → navigates to `//Login/LoginPage`

### `AppShell.xaml`
Defines two `TabBar` sections:

| Route | Description |
|---|---|
| `//Login/LoginPage` | Login screen (no nav bar, no tab bar) |
| `//Main/Dashboard` | Dashboard tab |
| `//Main/Users` | Users tab |
| `UserDetail` | Registered as a Shell route (push navigation) for create/edit |

Navigation between sections uses `Shell.Current.GoToAsync(...)`. Any `401 Unauthorized` response from the API automatically redirects to `//Login/LoginPage` and clears stored credentials.

---

## Third-Party API Integration

All communication with the Laravel backend is encapsulated in **`ApiClient`**.

### Base Configuration

```
Base URL:    http://localhost:8000   (configurable in MauiProgram.cs)
Headers:     Accept: application/json
             Authorization: Bearer <token>   (set per-request after login)
```

### Endpoints Used

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/login` | Authenticate user, returns token + user info |
| `POST` | `/api/logout` | Invalidate server-side token |
| `GET` | `/api/user` | Get current authenticated user |
| `GET` | `/api/users?page=N&role=X&search=Y` | Paginated, filtered user list |
| `GET` | `/api/users/{id}` | Get single user by ID |
| `POST` | `/api/users` | Create new user |
| `PUT` | `/api/users/{id}` | Update existing user |
| `DELETE` | `/api/users/{id}` | Delete user |
| `GET` | `/api/health` | Server health check (returns 2xx if online) |

### Request/Response Flow

1. Before any authenticated request, `SetAuthHeaderAsync()` reads the stored Bearer token from `AuthTokenStore` and sets it on `HttpClient.DefaultRequestHeaders.Authorization`.
2. Responses are deserialized using `System.Text.Json` with `PropertyNameCaseInsensitive = true` to handle Laravel's snake_case JSON fields (e.g., `created_at`, `last_page`).
3. Non-2xx responses are read as `ApiErrorResponse` and thrown as `ApiException`, which carries the HTTP status code and a `ValidationErrors` dictionary (matching Laravel's validation error format).

### Error Handling

`ApiException` exposes three convenience flags:

| Property | HTTP Status |
|---|---|
| `IsUnauthorized` | 401 |
| `IsForbidden` | 403 |
| `IsValidationError` | 422 |

ViewModels catch these specifically. A `401` always triggers token clearance and redirects to login.

### `CreateUserAsync` — Response Unwrapping
The create endpoint may return a `{ "data": { ... } }` wrapper (Laravel resource format) or a bare user object. `ApiClient` handles both cases using `JsonElement` inspection before deserializing.

---

## Authentication

### Login Flow

1. User enters email and password in `LoginPage`.
2. `LoginViewModel.LoginAsync()` sends a `POST /api/login` request via `ApiClient`.
3. On success:
   - The returned Bearer token is saved via `AuthTokenStore.SaveTokenAsync()`.
   - The user's `id`, `name`, and `email` are saved via `AuthTokenStore.SaveUserInfoAsync()`.
   - Navigation proceeds to `//Main/Dashboard`.
4. On failure, an error banner is shown in the UI (bound to `HasError` / `ErrorMessage`).

### Token Persistence

`AuthTokenStore` uses **`Microsoft.Maui.Storage.Preferences`** (platform-native key-value store):

| Key | Value |
|---|---|
| `auth_token` | Bearer token string |
| `auth_user_id` | User ID (as string) |
| `auth_user_name` | User display name |
| `auth_user_email` | User email address |

`HasTokenAsync()` is called at startup to decide whether to skip the login screen.

### Logout Flow

1. `DashboardViewModel.LogoutAsync()` calls `ApiClient.LogoutAsync()`, which calls `POST /api/logout`.
2. Regardless of whether the API call succeeds (network may be unavailable), `AuthTokenStore.Clear()` removes all stored credentials and the `Authorization` header is cleared from `HttpClient`.
3. Navigation redirects to `//Login/LoginPage`.

---

## Screens & Features

### LoginPage

- Email entry with `Keyboard="Email"` and clear button
- Password entry with `IsPassword="True"`
- `Return` key on password field triggers login
- Login button is disabled while busy (bound via `InvertBoolConverter`)
- Button text changes dynamically: `"Sign In"` → `"Signing in..."` (via `BusyTextConverter`)
- Red error banner (`Border`) is shown only when `HasError = true`
- `ActivityIndicator` is visible while `IsBusy = true`

### DashboardPage

- Personalised welcome message loaded from cached user info (e.g., `"Welcome, Daniel"`)
- **Server status indicator**: calls `GET /api/health`; dot turns green/red with text `"Server Online"` / `"Server Offline"` (via `StatusColorConverter` and `StatusTextConverter`)
- **Total Users** count loaded from `GET /api/users`
- `RefreshView` allows pull-to-refresh to reload all stats
- **Logout** button in the custom `Shell.TitleView`
- Navigation button to the Users screen

### UsersPage

- **Search bar** with real-time `ReturnCommand` wired to `SearchCommand`
- **Role filter Picker** with options: `All`, `user`, `mechanic`, `admin`
- **Paginated list** (`CollectionView`) displaying name, email, and role badge per user
- **Edit** button per row → navigates to `UserDetail?userId={id}`
- **Delete** button per row → shows a confirmation `DisplayAlert` before calling `DELETE /api/users/{id}` and removing the item from the `ObservableCollection<User>` locally
- **Pagination controls**: Previous / Next buttons with a label showing `Page X of Y (Z total users)`
- **"+ New User"** button → navigates to `UserDetail` (no query parameter = create mode)
- Empty state view when no results match

### UserDetailPage

- Dual-mode: **Create** (title = `"Create User"`) or **Edit** (title = `"Edit User"`), determined by the `userId` query parameter
- Fields: Name, Email, Password, Role (Picker)
- Password field label changes text depending on mode (`"Password"` vs `"Password (leave blank to keep current)"`) via `BusyTextConverter` with `IsExistingUser` as the binding value
- On save:
  - **Create**: requires all fields including password; calls `POST /api/users`
  - **Update**: calls `PUT /api/users/{id}`; password is only included in the request if the field is non-empty
- Validation errors from the API (422) are displayed in the error banner with all messages joined by newline
- Cancel button pops back to the previous page (`Shell.Current.GoToAsync("..")`)

---

## Data Models

### `User`

| Property | JSON Key | Type |
|---|---|---|
| `Id` | `id` | `int` |
| `Name` | `name` | `string` |
| `Email` | `email` | `string` |
| `CreatedAt` | `created_at` | `DateTime?` |
| `UpdatedAt` | `updated_at` | `DateTime?` |
| `Roles` | `roles` | `List<Role>` |
| `RoleDisplay` | *(computed)* | `string` — comma-joined role names |

### `Role`

| Property | JSON Key | Type |
|---|---|---|
| `Id` | `id` | `int` |
| `Name` | `name` | `string` |

The backend uses a many-to-many role relationship (Spatie Laravel Permission or equivalent). The app only reads the first role for display but sends a single `role` string on create/update.

### `PaginatedResponse<T>`

Matches Laravel's default paginator JSON structure:

| Property | JSON Key |
|---|---|
| `Data` | `data` |
| `CurrentPage` | `current_page` |
| `LastPage` | `last_page` |
| `PerPage` | `per_page` |
| `Total` | `total` |

### `LoginRequest` / `LoginResponse`

`LoginRequest` maps `email` and `password`. `LoginResponse` maps `message`, `token` (Bearer string), and an embedded `user` object.

### `ApiErrorResponse`

Maps `message` (string) and `errors` (`Dictionary<string, List<string>>`), matching Laravel's validation error format.

---

## Value Converters

Defined in `Converters/Converters.cs` and registered as static resources in `App.xaml`.

| Converter | Input | Output | Usage |
|---|---|---|---|
| `InvertBoolConverter` | `bool` | `bool` | Disable button while busy |
| `BusyTextConverter` | `bool` + `"TextA\|TextB"` parameter | `string` | Dynamic button text / label text |
| `StatusColorConverter` | `bool` (`IsServerOnline`) | `Color` | Green / Red dot on dashboard |
| `StatusTextConverter` | `bool` (`IsServerOnline`) | `string` | `"Server Online"` / `"Server Offline"` |

---

## Theming & Styling

The app uses a **dark theme** with a purple primary color. All colors are defined as static resources in `Resources/Styles/Colors.xaml`:

| Key | Hex | Role |
|---|---|---|
| `Primary` | `#512BD4` | Buttons, highlights, active role labels |
| `Gray950` | `#141414` | Page background (login) |
| `Gray900` | `#212121` | Card / border backgrounds |
| `Gray600` | `#404040` | Secondary buttons |
| `Gray400` | `#919191` | Muted text (email, subtitles) |
| `Gray300` | `#ACACAC` | Form labels |
| `Gray200` | `#C8C8C8` | Input labels |
| `Red100Accent` | `#FF5252` | Error banners, delete buttons |

XAML is compiled at build time using the **`MauiXamlInflator=SourceGen`** setting, which generates C# code from XAML (as seen in the `Views_LoginPage.xaml.xsg.cs` file), improving startup performance and enabling AOT compatibility.

---

## What Has Been Achieved

| Feature | Status |
|---|---|
| Cross-platform MAUI project (Android, iOS, macOS, Windows) | ✅ Complete |
| DI container wiring (services, viewmodels, pages) | ✅ Complete |
| Persistent session (auto-login on relaunch if token exists) | ✅ Complete |
| Login with Laravel Bearer token authentication | ✅ Complete |
| Logout with server-side token invalidation | ✅ Complete |
| Automatic redirect to login on 401 responses | ✅ Complete |
| Dashboard with server health check (`/api/health`) | ✅ Complete |
| Dashboard with total user count | ✅ Complete |
| Pull-to-refresh on dashboard | ✅ Complete |
| Paginated user list with server-driven pagination | ✅ Complete |
| User search by keyword | ✅ Complete |
| User filter by role | ✅ Complete |
| Create new user (with role assignment) | ✅ Complete |
| Edit existing user (name, email, password, role) | ✅ Complete |
| Delete user with confirmation dialog | ✅ Complete |
| Form validation with server error display (422 responses) | ✅ Complete |
| Busy state management across all screens | ✅ Complete |
| XAML source-generated compilation | ✅ Complete |
| Dark theme with consistent color palette | ✅ Complete |

### Pending / Known TODOs

- **Backend URL** is hardcoded to `http://localhost:8000` in `MauiProgram.cs` — should be moved to an environment/configuration file for production deployment.
- No unit or integration tests are present in the solution.
- No refresh token mechanism — if the Bearer token expires server-side, the user is simply redirected to login.
- The `GetCurrentUserAsync()` method (`GET /api/user`) is defined in `ApiClient` but not currently called from any ViewModel (user info is loaded from cached `Preferences` on the dashboard instead).
