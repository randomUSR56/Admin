using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class CarsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public CarsViewModel(IApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public ObservableCollection<Car> Cars { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _lastPage = 1;

    [ObservableProperty]
    private int _totalCars;

    [RelayCommand]
    private async Task LoadCarsAsync()
    {
        IsBusy = true;
        HasError = false;

        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            var result = await _apiClient.GetCarsAsync(CurrentPage, null, search);

            Cars.Clear();
            foreach (var car in result.Data)
                Cars.Add(car);

            LastPage = result.LastPage;
            TotalCars = result.Total;
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
        await LoadCarsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            CurrentPage++;
            await LoadCarsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCarsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteCarAsync(Car car)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirm Delete",
            $"Are you sure you want to delete \"{car.DisplayName}\"?",
            "Delete", "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _apiClient.DeleteCarAsync(car.Id);
            Cars.Remove(car);
            TotalCars--;
        }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ViewCarAsync(Car car)
    {
        await Shell.Current.GoToAsync($"CarDetail?carId={car.Id}");
    }

    [RelayCommand]
    private async Task CreateCarAsync()
    {
        await Shell.Current.GoToAsync("CarDetail");
    }
}
