using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class TicketModelTests
{
    [Fact]
    public void Ticket_DefaultValues_AreCorrect()
    {
        var ticket = new Ticket();
        Assert.Equal(0, ticket.Id);
        Assert.Equal(0, ticket.UserId);
        Assert.Null(ticket.MechanicId);
        Assert.Equal(0, ticket.CarId);
        Assert.Equal(string.Empty, ticket.Status);
        Assert.Equal(string.Empty, ticket.Priority);
        Assert.Equal(string.Empty, ticket.Description);
        Assert.Null(ticket.AcceptedAt);
        Assert.Null(ticket.CompletedAt);
        Assert.Null(ticket.CreatedAt);
        Assert.Null(ticket.UpdatedAt);
        Assert.Null(ticket.User);
        Assert.Null(ticket.Mechanic);
        Assert.Null(ticket.Car);
        Assert.Null(ticket.Problems);
    }

    [Fact]
    public void Ticket_Deserializes_FromSnakeCaseJson()
    {
        var json = """
        {
            "id": 42,
            "user_id": 5,
            "mechanic_id": 10,
            "car_id": 3,
            "status": "in_progress",
            "priority": "high",
            "description": "Engine overheating",
            "accepted_at": "2025-06-10T09:00:00.000000Z",
            "completed_at": null,
            "created_at": "2025-06-09T08:00:00.000000Z",
            "updated_at": "2025-06-10T09:00:00.000000Z"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var ticket = JsonSerializer.Deserialize<Ticket>(json, options);

        Assert.NotNull(ticket);
        Assert.Equal(42, ticket.Id);
        Assert.Equal(5, ticket.UserId);
        Assert.Equal(10, ticket.MechanicId);
        Assert.Equal(3, ticket.CarId);
        Assert.Equal("in_progress", ticket.Status);
        Assert.Equal("high", ticket.Priority);
        Assert.Equal("Engine overheating", ticket.Description);
        Assert.NotNull(ticket.AcceptedAt);
        Assert.Null(ticket.CompletedAt);
    }

    [Fact]
    public void Ticket_Deserializes_WithNullOptionalFields()
    {
        var json = """
        {
            "id": 1,
            "user_id": 1,
            "mechanic_id": null,
            "car_id": 2,
            "status": "open",
            "priority": "medium",
            "description": "Basic check",
            "accepted_at": null,
            "completed_at": null
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var ticket = JsonSerializer.Deserialize<Ticket>(json, options);

        Assert.NotNull(ticket);
        Assert.Null(ticket.MechanicId);
        Assert.Null(ticket.AcceptedAt);
        Assert.Null(ticket.CompletedAt);
    }

    [Fact]
    public void Ticket_Deserializes_WithNestedRelationships()
    {
        var json = """
        {
            "id": 10,
            "user_id": 2,
            "mechanic_id": 5,
            "car_id": 7,
            "status": "assigned",
            "priority": "urgent",
            "description": "Brake failure",
            "user": {
                "id": 2,
                "name": "Jane Doe",
                "email": "jane@example.com"
            },
            "mechanic": {
                "id": 5,
                "name": "Mike Mechanic",
                "email": "mike@example.com"
            },
            "car": {
                "id": 7,
                "user_id": 2,
                "make": "Toyota",
                "model": "Corolla",
                "year": 2024,
                "license_plate": "ABC-123"
            },
            "problems": [
                {
                    "id": 3,
                    "name": "Worn brake pads",
                    "category": "brakes",
                    "is_active": true,
                    "pivot": {
                        "ticket_id": 10,
                        "problem_id": 3,
                        "notes": "Front left"
                    }
                }
            ]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var ticket = JsonSerializer.Deserialize<Ticket>(json, options);

        Assert.NotNull(ticket);
        Assert.NotNull(ticket.User);
        Assert.Equal("Jane Doe", ticket.User.Name);
        Assert.NotNull(ticket.Mechanic);
        Assert.Equal("Mike Mechanic", ticket.Mechanic.Name);
        Assert.NotNull(ticket.Car);
        Assert.Equal("Toyota", ticket.Car.Make);
        Assert.NotNull(ticket.Problems);
        Assert.Single(ticket.Problems);
        Assert.Equal("Worn brake pads", ticket.Problems[0].Name);
        Assert.NotNull(ticket.Problems[0].Pivot);
        Assert.Equal("Front left", ticket.Problems[0].Pivot!.Notes);
    }

    [Theory]
    [InlineData("open", "Open")]
    [InlineData("assigned", "Assigned")]
    [InlineData("in_progress", "In Progress")]
    [InlineData("completed", "Completed")]
    [InlineData("closed", "Closed")]
    [InlineData("unknown", "unknown")]
    public void Ticket_StatusDisplay_ReturnsCorrectLabel(string status, string expected)
    {
        var ticket = new Ticket { Status = status };
        Assert.Equal(expected, ticket.StatusDisplay);
    }

    [Theory]
    [InlineData("low", "Low")]
    [InlineData("medium", "Medium")]
    [InlineData("high", "High")]
    [InlineData("urgent", "Urgent")]
    [InlineData("critical", "critical")]
    public void Ticket_PriorityDisplay_ReturnsCorrectLabel(string priority, string expected)
    {
        var ticket = new Ticket { Priority = priority };
        Assert.Equal(expected, ticket.PriorityDisplay);
    }

    [Fact]
    public void Ticket_OwnerDisplay_ShowsUserName()
    {
        var ticket = new Ticket { UserId = 5, User = new User { Name = "Alice" } };
        Assert.Equal("Alice", ticket.OwnerDisplay);
    }

    [Fact]
    public void Ticket_OwnerDisplay_FallsBackToUserId()
    {
        var ticket = new Ticket { UserId = 5 };
        Assert.Equal("User #5", ticket.OwnerDisplay);
    }

    [Fact]
    public void Ticket_MechanicDisplay_ShowsMechanicName()
    {
        var ticket = new Ticket { MechanicId = 10, Mechanic = new User { Name = "Bob" } };
        Assert.Equal("Bob", ticket.MechanicDisplay);
    }

    [Fact]
    public void Ticket_MechanicDisplay_FallsBackToMechanicId()
    {
        var ticket = new Ticket { MechanicId = 10 };
        Assert.Equal("Mechanic #10", ticket.MechanicDisplay);
    }

    [Fact]
    public void Ticket_MechanicDisplay_ShowsUnassigned()
    {
        var ticket = new Ticket();
        Assert.Equal("Unassigned", ticket.MechanicDisplay);
    }

    [Fact]
    public void Ticket_CarDisplay_ShowsCarDisplayName()
    {
        var ticket = new Ticket { CarId = 7, Car = new Car { Year = 2024, Make = "BMW", Model = "X5" } };
        Assert.Equal("2024 BMW X5", ticket.CarDisplay);
    }

    [Fact]
    public void Ticket_CarDisplay_FallsBackToCarId()
    {
        var ticket = new Ticket { CarId = 7 };
        Assert.Equal("Car #7", ticket.CarDisplay);
    }

    // ── Status-aware workflow flags ──

    [Theory]
    [InlineData("open", true)]
    [InlineData("assigned", false)]
    [InlineData("in_progress", false)]
    [InlineData("completed", false)]
    [InlineData("closed", false)]
    public void Ticket_CanAccept_OnlyWhenOpen(string status, bool expected)
    {
        var ticket = new Ticket { Status = status };
        Assert.Equal(expected, ticket.CanAccept);
    }

    [Theory]
    [InlineData("open", false)]
    [InlineData("assigned", true)]
    [InlineData("in_progress", false)]
    [InlineData("completed", false)]
    [InlineData("closed", false)]
    public void Ticket_CanStart_OnlyWhenAssigned(string status, bool expected)
    {
        var ticket = new Ticket { Status = status };
        Assert.Equal(expected, ticket.CanStart);
    }

    [Theory]
    [InlineData("open", false)]
    [InlineData("assigned", false)]
    [InlineData("in_progress", true)]
    [InlineData("completed", false)]
    [InlineData("closed", false)]
    public void Ticket_CanComplete_OnlyWhenInProgress(string status, bool expected)
    {
        var ticket = new Ticket { Status = status };
        Assert.Equal(expected, ticket.CanComplete);
    }

    [Theory]
    [InlineData("open", true)]
    [InlineData("assigned", true)]
    [InlineData("in_progress", true)]
    [InlineData("completed", false)]
    [InlineData("closed", false)]
    public void Ticket_CanClose_NotWhenCompletedOrClosed(string status, bool expected)
    {
        var ticket = new Ticket { Status = status };
        Assert.Equal(expected, ticket.CanClose);
    }
}
