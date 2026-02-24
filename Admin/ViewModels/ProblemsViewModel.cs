using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class ProblemsViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public ProblemsViewModel(ApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public ObservableCollection<Problem> Problems { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string? _selectedActiveFilter;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _lastPage = 1;

    [ObservableProperty]
    private int _totalProblems;

    public string[] AvailableCategories { get; } =
        ["All", "engine", "transmission", "electrical", "brakes", "suspension", "steering", "body", "other"];

    public string[] ActiveFilters { get; } = ["All", "Active", "Inactive"];

    [RelayCommand]
    private async Task LoadProblemsAsync()
    {
        IsBusy = true;
        HasError = false;

        try
        {
            var category = SelectedCategory is null or "All" ? null : SelectedCategory;
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            bool? isActive = SelectedActiveFilter switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            var result = await _apiClient.GetProblemsAsync(CurrentPage, category, isActive, search);

            Problems.Clear();
            foreach (var problem in result.Data)
                Problems.Add(problem);

            LastPage = result.LastPage;
            TotalProblems = result.Total;
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
        await LoadProblemsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            CurrentPage++;
            await LoadProblemsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProblemsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteProblemAsync(Problem problem)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirm Delete",
            $"Are you sure you want to delete \"{problem.Name}\"?",
            "Delete", "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _apiClient.DeleteProblemAsync(problem.Id);
            Problems.Remove(problem);
            TotalProblems--;
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
    private async Task ViewProblemAsync(Problem problem)
    {
        await Shell.Current.GoToAsync($"ProblemDetail?problemId={problem.Id}");
    }

    [RelayCommand]
    private async Task CreateProblemAsync()
    {
        await Shell.Current.GoToAsync("ProblemDetail");
    }
}
