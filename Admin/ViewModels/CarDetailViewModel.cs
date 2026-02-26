using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

[QueryProperty(nameof(CarId), "carId")]
public partial class CarDetailViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public CarDetailViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private int _carId;

    [ObservableProperty]
    private int _userId;

    [ObservableProperty]
    private string _make = string.Empty;

    [ObservableProperty]
    private string _model = string.Empty;

    [ObservableProperty]
    private int _year = DateTime.Now.Year;

    [ObservableProperty]
    private string _licensePlate = string.Empty;

    [ObservableProperty]
    private string? _vin;

    [ObservableProperty]
    private string? _color;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _pageTitle = "Create Car";

    [ObservableProperty]
    private bool _isExistingCar;

    partial void OnCarIdChanged(int value)
    {
        if (value > 0)
        {
            IsExistingCar = true;
            PageTitle = "Edit Car";
            LoadCarCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadCarAsync()
    {
        if (CarId <= 0) return;

        IsBusy = true;
        HasError = false;

        try
        {
            var car = await _apiClient.GetCarAsync(CarId);
            UserId = car.UserId;
            Make = car.Make;
            Model = car.Model;
            Year = car.Year;
            LicensePlate = car.LicensePlate;
            Vin = car.Vin;
            Color = car.Color;
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
    private async Task SaveAsync()
    {
        if (UserId <= 0)
        {
            HasError = true;
            ErrorMessage = "User ID is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Make))
        {
            HasError = true;
            ErrorMessage = "Make is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Model))
        {
            HasError = true;
            ErrorMessage = "Model is required.";
            return;
        }

        if (Year < 1900 || Year > DateTime.Now.Year + 2)
        {
            HasError = true;
            ErrorMessage = "Year must be between 1900 and " + (DateTime.Now.Year + 2) + ".";
            return;
        }

        if (string.IsNullOrWhiteSpace(LicensePlate))
        {
            HasError = true;
            ErrorMessage = "License plate is required.";
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            if (IsExistingCar)
            {
                var request = new UpdateCarRequest
                {
                    UserId = UserId,
                    Make = Make.Trim(),
                    Model = Model.Trim(),
                    Year = Year,
                    LicensePlate = LicensePlate.Trim(),
                    Vin = Vin?.Trim(),
                    Color = Color?.Trim()
                };

                await _apiClient.UpdateCarAsync(CarId, request);
            }
            else
            {
                var request = new CreateCarRequest
                {
                    UserId = UserId,
                    Make = Make.Trim(),
                    Model = Model.Trim(),
                    Year = Year,
                    LicensePlate = LicensePlate.Trim(),
                    Vin = Vin?.Trim(),
                    Color = Color?.Trim()
                };

                await _apiClient.CreateCarAsync(request);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            HasError = true;
            if (ex.IsValidationError && ex.ValidationErrors is not null)
            {
                var messages = ex.ValidationErrors.SelectMany(e => e.Value);
                ErrorMessage = string.Join("\n", messages);
            }
            else
            {
                ErrorMessage = ex.Message;
            }
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
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
