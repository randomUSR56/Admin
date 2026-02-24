using System.Text.Json.Serialization;

namespace Admin.Models;

public class Ticket
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("mechanic_id")]
    public int? MechanicId { get; set; }

    [JsonPropertyName("car_id")]
    public int CarId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("accepted_at")]
    public DateTime? AcceptedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Relationships
    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("mechanic")]
    public User? Mechanic { get; set; }

    [JsonPropertyName("car")]
    public Car? Car { get; set; }

    [JsonPropertyName("problems")]
    public List<Problem>? Problems { get; set; }

    // Computed display helpers
    public string StatusDisplay => Status switch
    {
        "open" => "Open",
        "assigned" => "Assigned",
        "in_progress" => "In Progress",
        "completed" => "Completed",
        "closed" => "Closed",
        _ => Status
    };

    public string PriorityDisplay => Priority switch
    {
        "low" => "Low",
        "medium" => "Medium",
        "high" => "High",
        "urgent" => "Urgent",
        _ => Priority
    };

    public string OwnerDisplay => User?.Name ?? $"User #{UserId}";

    public string MechanicDisplay => Mechanic?.Name ?? (MechanicId.HasValue ? $"Mechanic #{MechanicId}" : "Unassigned");

    public string CarDisplay => Car?.DisplayName ?? $"Car #{CarId}";
}
