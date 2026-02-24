using Admin.Models;
using Admin.Services;
using Moq;

namespace Admin.Tests.ViewModels;

/// <summary>
/// Tests for the Tickets API client methods via the IApiClient interface.
/// These tests validate the contract and mock behavior patterns used by the ViewModels.
/// </summary>
public class TicketsApiClientTests
{
    private readonly Mock<IApiClient> _mockApiClient;

    public TicketsApiClientTests()
    {
        _mockApiClient = new Mock<IApiClient>();
    }

    // ──────────────────────────────────────
    // GetTicketsAsync - list / pagination
    // ──────────────────────────────────────

    [Fact]
    public async Task GetTicketsAsync_ReturnsEmptyPaginatedResponse()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 0
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, null, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync();

        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsPaginatedTickets()
    {
        var tickets = new List<Ticket>
        {
            new() { Id = 1, UserId = 1, CarId = 2, Status = "open", Priority = "high", Description = "Engine noise" },
            new() { Id = 2, UserId = 2, CarId = 3, Status = "assigned", Priority = "medium", Description = "Oil change" },
            new() { Id = 3, UserId = 1, CarId = 4, Status = "completed", Priority = "low", Description = "Tire rotation" }
        };

        var expected = new PaginatedResponse<Ticket>
        {
            Data = tickets,
            CurrentPage = 1,
            LastPage = 3,
            PerPage = 15,
            Total = 35
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, null, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync();

        Assert.Equal(3, result.Data.Count);
        Assert.Equal(3, result.LastPage);
        Assert.Equal(35, result.Total);
    }

    [Fact]
    public async Task GetTicketsAsync_Page2_ReturnsCorrectPage()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 16, UserId = 1, CarId = 2, Status = "open", Priority = "medium", Description = "Page 2 ticket" }],
            CurrentPage = 2,
            LastPage = 4,
            PerPage = 15,
            Total = 50
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(2, null, null, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(2);

        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(4, result.LastPage);
    }

    // ──────────────────────────────────────
    // GetTicketsAsync - filters
    // ──────────────────────────────────────

    [Fact]
    public async Task GetTicketsAsync_WithStatusFilter()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 1, Status = "open", Priority = "high", Description = "Open ticket" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, "open", null, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, "open");

        Assert.Single(result.Data);
        Assert.Equal("open", result.Data[0].Status);
    }

    [Fact]
    public async Task GetTicketsAsync_WithPriorityFilter()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 2, Status = "assigned", Priority = "urgent", Description = "Urgent ticket" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, "urgent", null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, null, "urgent");

        Assert.Single(result.Data);
        Assert.Equal("urgent", result.Data[0].Priority);
    }

    [Fact]
    public async Task GetTicketsAsync_WithMechanicIdFilter()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 5, MechanicId = 10, Status = "in_progress", Priority = "medium", Description = "Mechanic assigned" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, null, 10, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, null, null, 10);

        Assert.Single(result.Data);
        Assert.Equal(10, result.Data[0].MechanicId);
    }

    [Fact]
    public async Task GetTicketsAsync_WithUserIdFilter()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 3, UserId = 7, CarId = 1, Status = "open", Priority = "low", Description = "User ticket" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, null, null, 7, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, null, null, null, 7);

        Assert.Single(result.Data);
        Assert.Equal(7, result.Data[0].UserId);
    }

    [Fact]
    public async Task GetTicketsAsync_WithCarIdFilter()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 8, UserId = 1, CarId = 15, Status = "assigned", Priority = "high", Description = "Car ticket" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, null, null, null, null, 15))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, null, null, null, null, 15);

        Assert.Single(result.Data);
        Assert.Equal(15, result.Data[0].CarId);
    }

    [Fact]
    public async Task GetTicketsAsync_WithMultipleFilters()
    {
        var expected = new PaginatedResponse<Ticket>
        {
            Data = [new() { Id = 12, UserId = 3, CarId = 5, MechanicId = 8, Status = "in_progress", Priority = "high", Description = "Multi-filter" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetTicketsAsync(1, "in_progress", "high", 8, 3, 5))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketsAsync(1, "in_progress", "high", 8, 3, 5);

        Assert.Single(result.Data);
        Assert.Equal("in_progress", result.Data[0].Status);
        Assert.Equal("high", result.Data[0].Priority);
        Assert.Equal(8, result.Data[0].MechanicId);
        Assert.Equal(3, result.Data[0].UserId);
        Assert.Equal(5, result.Data[0].CarId);
    }

    // ──────────────────────────────────────
    // GetTicketAsync - single ticket
    // ──────────────────────────────────────

    [Fact]
    public async Task GetTicketAsync_ReturnsTicket()
    {
        var expected = new Ticket
        {
            Id = 42,
            UserId = 5,
            MechanicId = 10,
            CarId = 3,
            Status = "in_progress",
            Priority = "high",
            Description = "Engine overheating"
        };

        _mockApiClient
            .Setup(c => c.GetTicketAsync(42))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketAsync(42);

        Assert.Equal(42, result.Id);
        Assert.Equal("in_progress", result.Status);
        Assert.Equal("high", result.Priority);
        Assert.Equal("Engine overheating", result.Description);
    }

    [Fact]
    public async Task GetTicketAsync_ReturnsTicketWithRelationships()
    {
        var expected = new Ticket
        {
            Id = 10,
            UserId = 2,
            MechanicId = 5,
            CarId = 7,
            Status = "assigned",
            Priority = "urgent",
            Description = "Brake failure",
            User = new User { Id = 2, Name = "Jane Doe", Email = "jane@example.com" },
            Mechanic = new User { Id = 5, Name = "Mike Mechanic", Email = "mike@example.com" },
            Car = new Car { Id = 7, Make = "Toyota", Model = "Corolla", Year = 2024, LicensePlate = "ABC-123" },
            Problems = [new Problem { Id = 3, Name = "Worn brake pads", Category = "brakes", IsActive = true }]
        };

        _mockApiClient
            .Setup(c => c.GetTicketAsync(10))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketAsync(10);

        Assert.NotNull(result.User);
        Assert.Equal("Jane Doe", result.User.Name);
        Assert.NotNull(result.Mechanic);
        Assert.Equal("Mike Mechanic", result.Mechanic.Name);
        Assert.NotNull(result.Car);
        Assert.Equal("Toyota", result.Car.Make);
        Assert.NotNull(result.Problems);
        Assert.Single(result.Problems);
    }

    [Fact]
    public async Task GetTicketAsync_ThrowsApiException_ForNotFound()
    {
        _mockApiClient
            .Setup(c => c.GetTicketAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.GetTicketAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    // ──────────────────────────────────────
    // CreateTicketAsync
    // ──────────────────────────────────────

    [Fact]
    public async Task CreateTicketAsync_ReturnsTicket()
    {
        var request = new CreateTicketRequest
        {
            CarId = 3,
            Description = "Engine overheating",
            Priority = "high",
            ProblemIds = [1, 5],
            ProblemNotes = ["Front left", "Rear"]
        };

        var expected = new Ticket
        {
            Id = 50,
            UserId = 1,
            CarId = 3,
            Status = "open",
            Priority = "high",
            Description = "Engine overheating"
        };

        _mockApiClient
            .Setup(c => c.CreateTicketAsync(request))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.CreateTicketAsync(request);

        Assert.Equal(50, result.Id);
        Assert.Equal("open", result.Status);
        Assert.Equal("high", result.Priority);
        Assert.Equal("Engine overheating", result.Description);
    }

    [Fact]
    public async Task CreateTicketAsync_ThrowsOnValidationError()
    {
        var request = new CreateTicketRequest { CarId = 0, Description = "" };
        var errors = new Dictionary<string, List<string>>
        {
            ["car_id"] = ["The car id field is required."],
            ["description"] = ["The description field is required."],
            ["problem_ids"] = ["The problem ids field is required."]
        };

        _mockApiClient
            .Setup(c => c.CreateTicketAsync(request))
            .ThrowsAsync(new ApiException("Validation failed", 422, errors));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.CreateTicketAsync(request));

        Assert.True(ex.IsValidationError);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Equal(3, ex.ValidationErrors.Count);
    }

    // ──────────────────────────────────────
    // UpdateTicketAsync
    // ──────────────────────────────────────

    [Fact]
    public async Task UpdateTicketAsync_Succeeds()
    {
        var request = new UpdateTicketRequest { Priority = "urgent", MechanicId = 10 };

        _mockApiClient
            .Setup(c => c.UpdateTicketAsync(42, request))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.UpdateTicketAsync(42, request);

        _mockApiClient.Verify(c => c.UpdateTicketAsync(42, request), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketAsync_ThrowsOnUnauthorized()
    {
        var request = new UpdateTicketRequest { Description = "Updated" };

        _mockApiClient
            .Setup(c => c.UpdateTicketAsync(1, request))
            .ThrowsAsync(new ApiException("Unauthorized", 401));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.UpdateTicketAsync(1, request));

        Assert.True(ex.IsUnauthorized);
    }

    // ──────────────────────────────────────
    // DeleteTicketAsync
    // ──────────────────────────────────────

    [Fact]
    public async Task DeleteTicketAsync_Succeeds()
    {
        _mockApiClient
            .Setup(c => c.DeleteTicketAsync(42))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.DeleteTicketAsync(42);

        _mockApiClient.Verify(c => c.DeleteTicketAsync(42), Times.Once);
    }

    [Fact]
    public async Task DeleteTicketAsync_ThrowsOnNotFound()
    {
        _mockApiClient
            .Setup(c => c.DeleteTicketAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.DeleteTicketAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    // ──────────────────────────────────────
    // Workflow actions
    // ──────────────────────────────────────

    [Fact]
    public async Task AcceptTicketAsync_ReturnsUpdatedTicket()
    {
        var expected = new Ticket
        {
            Id = 10,
            Status = "assigned",
            MechanicId = 5,
            AcceptedAt = DateTime.UtcNow
        };

        _mockApiClient
            .Setup(c => c.AcceptTicketAsync(10))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.AcceptTicketAsync(10);

        Assert.Equal("assigned", result.Status);
        Assert.NotNull(result.MechanicId);
        Assert.NotNull(result.AcceptedAt);
    }

    [Fact]
    public async Task StartTicketAsync_ReturnsUpdatedTicket()
    {
        var expected = new Ticket
        {
            Id = 10,
            Status = "in_progress",
            MechanicId = 5
        };

        _mockApiClient
            .Setup(c => c.StartTicketAsync(10))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.StartTicketAsync(10);

        Assert.Equal("in_progress", result.Status);
    }

    [Fact]
    public async Task CompleteTicketAsync_ReturnsUpdatedTicket()
    {
        var expected = new Ticket
        {
            Id = 10,
            Status = "completed",
            MechanicId = 5,
            CompletedAt = DateTime.UtcNow
        };

        _mockApiClient
            .Setup(c => c.CompleteTicketAsync(10))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.CompleteTicketAsync(10);

        Assert.Equal("completed", result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task CloseTicketAsync_ReturnsUpdatedTicket()
    {
        var expected = new Ticket
        {
            Id = 10,
            Status = "closed"
        };

        _mockApiClient
            .Setup(c => c.CloseTicketAsync(10))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.CloseTicketAsync(10);

        Assert.Equal("closed", result.Status);
    }

    [Fact]
    public async Task AcceptTicketAsync_ThrowsOnInvalidTransition()
    {
        _mockApiClient
            .Setup(c => c.AcceptTicketAsync(10))
            .ThrowsAsync(new ApiException("Cannot accept ticket in current status", 422));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.AcceptTicketAsync(10));

        Assert.True(ex.IsValidationError);
    }

    [Fact]
    public async Task StartTicketAsync_ThrowsOnNotFound()
    {
        _mockApiClient
            .Setup(c => c.StartTicketAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.StartTicketAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    // ──────────────────────────────────────
    // GetTicketStatisticsAsync
    // ──────────────────────────────────────

    [Fact]
    public async Task GetTicketStatisticsAsync_ReturnsStatistics()
    {
        var expected = new TicketStatistics
        {
            TotalTickets = 150,
            ByStatus = new TicketStatusCounts
            {
                Open = 30,
                Assigned = 25,
                InProgress = 40,
                Completed = 45,
                Closed = 10
            },
            ByPriority = new TicketPriorityCounts
            {
                Low = 20,
                Medium = 60,
                High = 50,
                Urgent = 20
            },
            OpenTickets = 30,
            AssignedTickets = 25,
            InProgressTickets = 40,
            CompletedToday = 8
        };

        _mockApiClient
            .Setup(c => c.GetTicketStatisticsAsync())
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetTicketStatisticsAsync();

        Assert.Equal(150, result.TotalTickets);
        Assert.Equal(30, result.ByStatus.Open);
        Assert.Equal(25, result.ByStatus.Assigned);
        Assert.Equal(40, result.ByStatus.InProgress);
        Assert.Equal(45, result.ByStatus.Completed);
        Assert.Equal(10, result.ByStatus.Closed);
        Assert.Equal(20, result.ByPriority.Low);
        Assert.Equal(60, result.ByPriority.Medium);
        Assert.Equal(50, result.ByPriority.High);
        Assert.Equal(20, result.ByPriority.Urgent);
        Assert.Equal(8, result.CompletedToday);
    }

    [Fact]
    public async Task GetTicketStatisticsAsync_ThrowsOnUnauthorized()
    {
        _mockApiClient
            .Setup(c => c.GetTicketStatisticsAsync())
            .ThrowsAsync(new ApiException("Unauthorized", 401));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.GetTicketStatisticsAsync());

        Assert.True(ex.IsUnauthorized);
    }
}
