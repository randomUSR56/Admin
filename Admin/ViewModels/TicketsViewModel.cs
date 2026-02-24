using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

public partial class TicketsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly AuthTokenStore _tokenStore;

    public TicketsViewModel(IApiClient apiClient, AuthTokenStore tokenStore)
    {
        _apiClient = apiClient;
        _tokenStore = tokenStore;
    }

    public ObservableCollection<Ticket> Tickets { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private string? _selectedPriority;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _lastPage = 1;

    [ObservableProperty]
    private int _totalTickets;

    public string[] AvailableStatuses { get; } =
        ["All", "open", "assigned", "in_progress", "completed", "closed"];

    public string[] AvailablePriorities { get; } =
        ["All", "low", "medium", "high", "urgent"];

    [RelayCommand]
    private async Task LoadTicketsAsync()
    {
        IsBusy = true;
        HasError = false;

        try
        {
            var status = SelectedStatus is null or "All" ? null : SelectedStatus;
            var priority = SelectedPriority is null or "All" ? null : SelectedPriority;

            var result = await _apiClient.GetTicketsAsync(CurrentPage, status, priority);

            Tickets.Clear();
            foreach (var ticket in result.Data)
                Tickets.Add(ticket);

            LastPage = result.LastPage;
            TotalTickets = result.Total;
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
    private async Task FilterAsync()
    {
        CurrentPage = 1;
        await LoadTicketsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < LastPage)
        {
            CurrentPage++;
            await LoadTicketsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadTicketsAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteTicketAsync(Ticket ticket)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirm Delete",
            $"Are you sure you want to delete ticket #{ticket.Id}?",
            "Delete", "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _apiClient.DeleteTicketAsync(ticket.Id);
            Tickets.Remove(ticket);
            TotalTickets--;
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
    private async Task AcceptTicketAsync(Ticket ticket)
    {
        IsBusy = true;
        try
        {
            var updated = await _apiClient.AcceptTicketAsync(ticket.Id);
            var index = Tickets.IndexOf(ticket);
            if (index >= 0)
                Tickets[index] = updated;
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
    private async Task StartTicketAsync(Ticket ticket)
    {
        IsBusy = true;
        try
        {
            var updated = await _apiClient.StartTicketAsync(ticket.Id);
            var index = Tickets.IndexOf(ticket);
            if (index >= 0)
                Tickets[index] = updated;
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
    private async Task CompleteTicketAsync(Ticket ticket)
    {
        IsBusy = true;
        try
        {
            var updated = await _apiClient.CompleteTicketAsync(ticket.Id);
            var index = Tickets.IndexOf(ticket);
            if (index >= 0)
                Tickets[index] = updated;
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
    private async Task CloseTicketAsync(Ticket ticket)
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Close Ticket",
            $"Are you sure you want to close ticket #{ticket.Id}?",
            "Close", "Cancel");

        if (!confirm) return;

        IsBusy = true;
        try
        {
            var updated = await _apiClient.CloseTicketAsync(ticket.Id);
            var index = Tickets.IndexOf(ticket);
            if (index >= 0)
                Tickets[index] = updated;
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
    private async Task ViewTicketAsync(Ticket ticket)
    {
        await Shell.Current.GoToAsync($"TicketDetail?ticketId={ticket.Id}");
    }

    [RelayCommand]
    private async Task CreateTicketAsync()
    {
        await Shell.Current.GoToAsync("TicketDetail");
    }
}
