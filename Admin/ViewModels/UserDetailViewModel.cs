using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class UserDetailViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

    public UserDetailViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private int _userId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _selectedRole = "user";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _pageTitle = "Create User";

    [ObservableProperty]
    private bool _isExistingUser;

    public string[] AvailableRoles { get; } = ["user", "mechanic", "admin"];

    partial void OnUserIdChanged(int value)
    {
        if (value > 0)
        {
            IsExistingUser = true;
            PageTitle = "Edit User";
            LoadUserCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadUserAsync()
    {
        if (UserId <= 0) return;

        IsBusy = true;
        HasError = false;

        try
        {
            var user = await _apiClient.GetUserAsync(UserId);
            Name = user.Name;
            Email = user.Email;

            if (user.Roles.Count > 0)
                SelectedRole = user.Roles[0].Name;
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
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
        {
            HasError = true;
            ErrorMessage = "Name and email are required.";
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            if (IsExistingUser)
            {
                var request = new UpdateUserRequest
                {
                    Name = Name.Trim(),
                    Email = Email.Trim(),
                    Role = SelectedRole
                };

                if (!string.IsNullOrWhiteSpace(Password))
                    request.Password = Password;

                await _apiClient.UpdateUserAsync(UserId, request);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    HasError = true;
                    ErrorMessage = "Password is required for new users.";
                    return;
                }

                var request = new CreateUserRequest
                {
                    Name = Name.Trim(),
                    Email = Email.Trim(),
                    Password = Password,
                    Role = SelectedRole
                };

                await _apiClient.CreateUserAsync(request);
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
