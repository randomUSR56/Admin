using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public DashboardViewModel(IApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    [ObservableProperty]
    private string _welcomeMessage = "Welcome";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isServerOnline;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private int _totalProblems;

    [ObservableProperty]
    private int _totalCars;

    [ObservableProperty]
    private int _totalTickets;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        HasError = false;

        try
        {
            var userInfo = await _tokenStore.GetUserInfoAsync();
            if (userInfo is not null)
                WelcomeMessage = $"Welcome, {userInfo.Value.Name}";

            IsServerOnline = await _apiClient.HealthCheckAsync();

            var users = await _apiClient.GetUsersAsync();
            TotalUsers = users.Total;

            var problems = await _apiClient.GetProblemsAsync();
            TotalProblems = problems.Total;

            var cars = await _apiClient.GetCarsAsync();
            TotalCars = cars.Total;

            var tickets = await _apiClient.GetTicketsAsync();
            TotalTickets = tickets.Total;
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            _tokenStore.Clear();
            await Shell.Current.GoToAsync("//LoginPage");
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
    private async Task NavigateToUsersAsync()
    {
        await Shell.Current.GoToAsync("//Users/UsersPage");
    }

    [RelayCommand]
    private async Task NavigateToProblemsAsync()
    {
        await Shell.Current.GoToAsync("//Problems/ProblemsPage");
    }

    [RelayCommand]
    private async Task NavigateToCarsAsync()
    {
        await Shell.Current.GoToAsync("//Cars/CarsPage");
    }

    [RelayCommand]
    private async Task NavigateToTicketsAsync()
    {
        await Shell.Current.GoToAsync("//Tickets/TicketsPage");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        try
        {
            await _apiClient.LogoutAsync();
        }
        catch
        {
            // Even if logout API fails, clear local state
            _tokenStore.Clear();
        }
        finally
        {
            IsBusy = false;
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
