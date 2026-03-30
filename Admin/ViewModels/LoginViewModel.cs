using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public LoginViewModel(IApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
        _serverUrl = _tokenStore.GetServerUrl();
    }

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    private bool CanLogin => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;
        IsBusy = true;

        // Save server URL if it was changed
        if (!string.IsNullOrWhiteSpace(ServerUrl))
        {
            _tokenStore.SaveServerUrl(ServerUrl.Trim());
        }

        try
        {
            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            };

            await _apiClient.LoginAsync(request);

            await Shell.Current.GoToAsync("//Dashboard/DashboardPage");
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
        catch (HttpRequestException)
        {
            HasError = true;
            ErrorMessage = "Cannot connect to server. Please check your connection.";
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "An unexpected error occurred.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
