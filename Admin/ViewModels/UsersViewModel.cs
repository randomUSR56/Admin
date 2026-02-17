using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class UsersViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public UsersViewModel(ApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public ObservableCollection<User> Users { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedRole;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _lastPage = 1;

    [ObservableProperty]
    private int _totalUsers;

    public string[] AvailableRoles { get; } = ["All", "user", "mechanic", "admin"];

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        HasError = false;

        try
        {
            var role = SelectedRole is null or "All" ? null : SelectedRole;
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            var result = await _apiClient.GetUsersAsync(CurrentPage, role, search);

            Users.Clear();
            foreach (var user in result.Data)
                Users.Add(user);

            LastPage = result.LastPage;
            TotalUsers = result.Total;
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            _tokenStore.Clear();
            await Shell.Current.GoToAsync("//Login/LoginPage");
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadUsersAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            CurrentPage++;
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User user)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {user.Name}?",
            "Delete", "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _apiClient.DeleteUserAsync(user.Id);
            Users.Remove(user);
            TotalUsers--;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ViewUserAsync(User user)
    {
        await Shell.Current.GoToAsync($"UserDetail?userId={user.Id}");
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        await Shell.Current.GoToAsync("UserDetail");
    }
}
