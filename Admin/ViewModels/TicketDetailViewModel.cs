using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Admin.Models;
using Admin.Services;

namespace Admin.ViewModels;

[QueryProperty(nameof(TicketId), "ticketId")]
public partial class TicketDetailViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;

    public TicketDetailViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private int _ticketId;

    [ObservableProperty]
    private int _carId;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedPriority = "medium";

    [ObservableProperty]
    private string _selectedStatus = "open";

    [ObservableProperty]
    private int? _mechanicId;

    [ObservableProperty]
    private string _problemIdsText = string.Empty;

    [ObservableProperty]
    private string _problemNotesText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _pageTitle = "Create Ticket";

    [ObservableProperty]
    private bool _isExistingTicket;

    // Read-only display fields for existing tickets
    [ObservableProperty]
    private string _ownerDisplay = string.Empty;

    [ObservableProperty]
    private string _mechanicDisplay = string.Empty;

    [ObservableProperty]
    private string _carDisplay = string.Empty;

    [ObservableProperty]
    private string _statusDisplay = string.Empty;

    [ObservableProperty]
    private string _createdAtDisplay = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Problem> _attachedProblems = [];

    public string[] AvailablePriorities { get; } = ["low", "medium", "high", "urgent"];

    public string[] AvailableStatuses { get; } = ["open", "assigned", "in_progress", "completed", "closed"];

    partial void OnTicketIdChanged(int value)
    {
        if (value > 0)
        {
            IsExistingTicket = true;
            PageTitle = "Edit Ticket";
            LoadTicketCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadTicketAsync()
    {
        if (TicketId <= 0) return;

        IsBusy = true;
        HasError = false;

        try
        {
            var ticket = await _apiClient.GetTicketAsync(TicketId);
            CarId = ticket.CarId;
            Description = ticket.Description;
            SelectedPriority = ticket.Priority;
            SelectedStatus = ticket.Status;
            MechanicId = ticket.MechanicId;

            OwnerDisplay = ticket.OwnerDisplay;
            MechanicDisplay = ticket.MechanicDisplay;
            CarDisplay = ticket.CarDisplay;
            StatusDisplay = ticket.StatusDisplay;
            CreatedAtDisplay = ticket.CreatedAt?.ToString("g") ?? "";

            AttachedProblems.Clear();
            if (ticket.Problems is not null)
            {
                foreach (var p in ticket.Problems)
                    AttachedProblems.Add(p);

                // Populate problem IDs and notes text for editing
                ProblemIdsText = string.Join(", ", ticket.Problems.Select(p => p.Id));
                ProblemNotesText = string.Join(", ", ticket.Problems.Select(p => p.Pivot?.Notes ?? ""));
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
    private async Task SaveAsync()
    {
        if (CarId <= 0)
        {
            HasError = true;
            ErrorMessage = "Car ID is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            HasError = true;
            ErrorMessage = "Description is required.";
            return;
        }

        var problemIds = ParseProblemIds();
        if (problemIds.Count == 0)
        {
            HasError = true;
            ErrorMessage = "At least one problem ID is required.";
            return;
        }

        var problemNotes = ParseProblemNotes(problemIds.Count);

        IsBusy = true;
        HasError = false;

        try
        {
            if (IsExistingTicket)
            {
                var request = new UpdateTicketRequest
                {
                    Description = Description.Trim(),
                    Priority = SelectedPriority,
                    Status = SelectedStatus,
                    MechanicId = MechanicId,
                    ProblemIds = problemIds,
                    ProblemNotes = problemNotes
                };

                await _apiClient.UpdateTicketAsync(TicketId, request);
            }
            else
            {
                var request = new CreateTicketRequest
                {
                    CarId = CarId,
                    Description = Description.Trim(),
                    Priority = SelectedPriority,
                    ProblemIds = problemIds,
                    ProblemNotes = problemNotes
                };

                await _apiClient.CreateTicketAsync(request);
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

    private List<int> ParseProblemIds()
    {
        var ids = new List<int>();
        if (string.IsNullOrWhiteSpace(ProblemIdsText)) return ids;

        foreach (var part in ProblemIdsText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var id) && id > 0)
                ids.Add(id);
        }
        return ids;
    }

    private List<string?> ParseProblemNotes(int expectedCount)
    {
        if (string.IsNullOrWhiteSpace(ProblemNotesText))
            return Enumerable.Repeat<string?>(null, expectedCount).ToList();

        var parts = ProblemNotesText.Split(',').Select(p => string.IsNullOrWhiteSpace(p) ? null : p.Trim()).ToList();

        // Pad to expected count
        while (parts.Count < expectedCount)
            parts.Add(null);

        return parts;
    }
}
