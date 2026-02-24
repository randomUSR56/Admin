using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

[QueryProperty(nameof(ProblemId), "problemId")]
public partial class ProblemDetailViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public ProblemDetailViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private int _problemId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "engine";

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _pageTitle = "Create Problem";

    [ObservableProperty]
    private bool _isExistingProblem;

    public string[] AvailableCategories { get; } =
        ["engine", "transmission", "electrical", "brakes", "suspension", "steering", "body", "other"];

    partial void OnProblemIdChanged(int value)
    {
        if (value > 0)
        {
            IsExistingProblem = true;
            PageTitle = "Edit Problem";
            LoadProblemCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadProblemAsync()
    {
        if (ProblemId <= 0) return;

        IsBusy = true;
        HasError = false;

        try
        {
            var problem = await _apiClient.GetProblemAsync(ProblemId);
            Name = problem.Name;
            SelectedCategory = problem.Category;
            Description = problem.Description;
            IsActive = problem.IsActive;
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
        if (string.IsNullOrWhiteSpace(Name))
        {
            HasError = true;
            ErrorMessage = "Name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedCategory))
        {
            HasError = true;
            ErrorMessage = "Category is required.";
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            if (IsExistingProblem)
            {
                var request = new UpdateProblemRequest
                {
                    Name = Name.Trim(),
                    Category = SelectedCategory,
                    Description = Description?.Trim(),
                    IsActive = IsActive
                };

                await _apiClient.UpdateProblemAsync(ProblemId, request);
            }
            else
            {
                var request = new CreateProblemRequest
                {
                    Name = Name.Trim(),
                    Category = SelectedCategory,
                    Description = Description?.Trim(),
                    IsActive = IsActive
                };

                await _apiClient.CreateProblemAsync(request);
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
