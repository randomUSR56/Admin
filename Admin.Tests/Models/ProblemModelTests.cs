using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class ProblemModelTests
{
    [Fact]
    public void Problem_ActiveDisplay_ReturnsActive_WhenIsActiveTrue()
    {
        var problem = new Problem { IsActive = true };
        Assert.Equal("Active", problem.ActiveDisplay);
    }

    [Fact]
    public void Problem_ActiveDisplay_ReturnsInactive_WhenIsActiveFalse()
    {
        var problem = new Problem { IsActive = false };
        Assert.Equal("Inactive", problem.ActiveDisplay);
    }

    [Fact]
    public void Problem_DefaultValues_AreCorrect()
    {
        var problem = new Problem();
        Assert.Equal(0, problem.Id);
        Assert.Equal(string.Empty, problem.Name);
        Assert.Equal(string.Empty, problem.Category);
        Assert.Null(problem.Description);
        Assert.False(problem.IsActive);
        Assert.Null(problem.CreatedAt);
        Assert.Null(problem.UpdatedAt);
        Assert.Null(problem.Pivot);
    }

    [Fact]
    public void Problem_Deserializes_FromSnakeCaseJson()
    {
        var json = """
        {
            "id": 5,
            "name": "Worn brake pads",
            "category": "brakes",
            "description": "Front or rear brake pads below minimum thickness.",
            "is_active": true,
            "created_at": "2025-06-01T10:00:00.000000Z",
            "updated_at": "2025-06-02T12:00:00.000000Z"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problem = JsonSerializer.Deserialize<Problem>(json, options);

        Assert.NotNull(problem);
        Assert.Equal(5, problem.Id);
        Assert.Equal("Worn brake pads", problem.Name);
        Assert.Equal("brakes", problem.Category);
        Assert.Equal("Front or rear brake pads below minimum thickness.", problem.Description);
        Assert.True(problem.IsActive);
        Assert.Equal("Active", problem.ActiveDisplay);
    }

    [Fact]
    public void Problem_Deserializes_WithPivotData()
    {
        var json = """
        {
            "id": 3,
            "name": "Engine overheating",
            "category": "engine",
            "is_active": true,
            "pivot": {
                "ticket_id": 10,
                "problem_id": 3,
                "notes": "Occurs after 30 min driving"
            }
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problem = JsonSerializer.Deserialize<Problem>(json, options);

        Assert.NotNull(problem);
        Assert.NotNull(problem.Pivot);
        Assert.Equal(10, problem.Pivot.TicketId);
        Assert.Equal(3, problem.Pivot.ProblemId);
        Assert.Equal("Occurs after 30 min driving", problem.Pivot.Notes);
    }

    [Fact]
    public void Problem_Deserializes_WithNullDescription()
    {
        var json = """
        {
            "id": 1,
            "name": "Test",
            "category": "other",
            "description": null,
            "is_active": false
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var problem = JsonSerializer.Deserialize<Problem>(json, options);

        Assert.NotNull(problem);
        Assert.Null(problem.Description);
        Assert.Equal("Inactive", problem.ActiveDisplay);
    }

    [Fact]
    public void ProblemPivot_DefaultValues_AreCorrect()
    {
        var pivot = new ProblemPivot();
        Assert.Equal(0, pivot.TicketId);
        Assert.Equal(0, pivot.ProblemId);
        Assert.Null(pivot.Notes);
    }
}
