using System.Text.Json.Serialization;

namespace Admin.Models;

public class CreateTicketRequest
{
    [JsonPropertyName("car_id")]
    public int CarId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = "medium";

    [JsonPropertyName("problem_ids")]
    public List<int> ProblemIds { get; set; } = [];

    [JsonPropertyName("problem_notes")]
    public List<string?> ProblemNotes { get; set; } = [];
}

public class UpdateTicketRequest
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("mechanic_id")]
    public int? MechanicId { get; set; }

    [JsonPropertyName("problem_ids")]
    public List<int>? ProblemIds { get; set; }

    [JsonPropertyName("problem_notes")]
    public List<string?>? ProblemNotes { get; set; }
}
