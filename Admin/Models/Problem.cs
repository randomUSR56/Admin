using System.Text.Json.Serialization;

namespace Admin.Models;

public class Problem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("pivot")]
    public ProblemPivot? Pivot { get; set; }

    public string ActiveDisplay => IsActive ? "Active" : "Inactive";
}

public class ProblemPivot
{
    [JsonPropertyName("ticket_id")]
    public int TicketId { get; set; }

    [JsonPropertyName("problem_id")]
    public int ProblemId { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
