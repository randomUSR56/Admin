using System.Text.Json.Serialization;

namespace Admin.Models;

public class TicketStatistics
{
    [JsonPropertyName("total_tickets")]
    public int TotalTickets { get; set; }

    [JsonPropertyName("by_status")]
    public TicketStatusCounts ByStatus { get; set; } = new();

    [JsonPropertyName("by_priority")]
    public TicketPriorityCounts ByPriority { get; set; } = new();

    [JsonPropertyName("open_tickets")]
    public int OpenTickets { get; set; }

    [JsonPropertyName("assigned_tickets")]
    public int AssignedTickets { get; set; }

    [JsonPropertyName("in_progress_tickets")]
    public int InProgressTickets { get; set; }

    [JsonPropertyName("completed_today")]
    public int CompletedToday { get; set; }
}

public class TicketStatusCounts
{
    [JsonPropertyName("open")]
    public int Open { get; set; }

    [JsonPropertyName("assigned")]
    public int Assigned { get; set; }

    [JsonPropertyName("in_progress")]
    public int InProgress { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("closed")]
    public int Closed { get; set; }
}

public class TicketPriorityCounts
{
    [JsonPropertyName("low")]
    public int Low { get; set; }

    [JsonPropertyName("medium")]
    public int Medium { get; set; }

    [JsonPropertyName("high")]
    public int High { get; set; }

    [JsonPropertyName("urgent")]
    public int Urgent { get; set; }
}
