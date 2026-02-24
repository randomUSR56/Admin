using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class TicketStatisticsTests
{
    [Fact]
    public void TicketStatistics_DefaultValues_AreCorrect()
    {
        var stats = new TicketStatistics();
        Assert.Equal(0, stats.TotalTickets);
        Assert.Equal(0, stats.OpenTickets);
        Assert.Equal(0, stats.AssignedTickets);
        Assert.Equal(0, stats.InProgressTickets);
        Assert.Equal(0, stats.CompletedToday);
        Assert.NotNull(stats.ByStatus);
        Assert.NotNull(stats.ByPriority);
    }

    [Fact]
    public void TicketStatistics_Deserializes_FromJson()
    {
        var json = """
        {
            "total_tickets": 150,
            "by_status": {
                "open": 30,
                "assigned": 25,
                "in_progress": 40,
                "completed": 45,
                "closed": 10
            },
            "by_priority": {
                "low": 20,
                "medium": 60,
                "high": 50,
                "urgent": 20
            },
            "open_tickets": 30,
            "assigned_tickets": 25,
            "in_progress_tickets": 40,
            "completed_today": 8
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stats = JsonSerializer.Deserialize<TicketStatistics>(json, options);

        Assert.NotNull(stats);
        Assert.Equal(150, stats.TotalTickets);
        Assert.Equal(30, stats.OpenTickets);
        Assert.Equal(25, stats.AssignedTickets);
        Assert.Equal(40, stats.InProgressTickets);
        Assert.Equal(8, stats.CompletedToday);
    }

    [Fact]
    public void TicketStatusCounts_DefaultValues_AreZero()
    {
        var counts = new TicketStatusCounts();
        Assert.Equal(0, counts.Open);
        Assert.Equal(0, counts.Assigned);
        Assert.Equal(0, counts.InProgress);
        Assert.Equal(0, counts.Completed);
        Assert.Equal(0, counts.Closed);
    }

    [Fact]
    public void TicketStatusCounts_Deserializes_FromJson()
    {
        var json = """
        {
            "open": 30,
            "assigned": 25,
            "in_progress": 40,
            "completed": 45,
            "closed": 10
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var counts = JsonSerializer.Deserialize<TicketStatusCounts>(json, options);

        Assert.NotNull(counts);
        Assert.Equal(30, counts.Open);
        Assert.Equal(25, counts.Assigned);
        Assert.Equal(40, counts.InProgress);
        Assert.Equal(45, counts.Completed);
        Assert.Equal(10, counts.Closed);
    }

    [Fact]
    public void TicketPriorityCounts_DefaultValues_AreZero()
    {
        var counts = new TicketPriorityCounts();
        Assert.Equal(0, counts.Low);
        Assert.Equal(0, counts.Medium);
        Assert.Equal(0, counts.High);
        Assert.Equal(0, counts.Urgent);
    }

    [Fact]
    public void TicketPriorityCounts_Deserializes_FromJson()
    {
        var json = """
        {
            "low": 20,
            "medium": 60,
            "high": 50,
            "urgent": 20
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var counts = JsonSerializer.Deserialize<TicketPriorityCounts>(json, options);

        Assert.NotNull(counts);
        Assert.Equal(20, counts.Low);
        Assert.Equal(60, counts.Medium);
        Assert.Equal(50, counts.High);
        Assert.Equal(20, counts.Urgent);
    }

    [Fact]
    public void TicketStatistics_Deserializes_NestedByStatus()
    {
        var json = """
        {
            "total_tickets": 50,
            "by_status": {
                "open": 10,
                "assigned": 8,
                "in_progress": 15,
                "completed": 12,
                "closed": 5
            },
            "by_priority": {
                "low": 5,
                "medium": 25,
                "high": 15,
                "urgent": 5
            },
            "open_tickets": 10,
            "assigned_tickets": 8,
            "in_progress_tickets": 15,
            "completed_today": 3
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stats = JsonSerializer.Deserialize<TicketStatistics>(json, options);

        Assert.NotNull(stats);
        Assert.Equal(10, stats.ByStatus.Open);
        Assert.Equal(8, stats.ByStatus.Assigned);
        Assert.Equal(15, stats.ByStatus.InProgress);
        Assert.Equal(12, stats.ByStatus.Completed);
        Assert.Equal(5, stats.ByStatus.Closed);

        Assert.Equal(5, stats.ByPriority.Low);
        Assert.Equal(25, stats.ByPriority.Medium);
        Assert.Equal(15, stats.ByPriority.High);
        Assert.Equal(5, stats.ByPriority.Urgent);
    }
}
