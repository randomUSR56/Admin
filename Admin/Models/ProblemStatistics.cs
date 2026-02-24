using System.Text.Json.Serialization;

namespace Admin.Models;

public class ProblemStatistics
{
    [JsonPropertyName("total_problems")]
    public int TotalProblems { get; set; }

    [JsonPropertyName("active_problems")]
    public int ActiveProblems { get; set; }

    [JsonPropertyName("problems_by_frequency")]
    public List<ProblemFrequency> ProblemsByFrequency { get; set; } = [];
}

public class ProblemFrequency
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("tickets_count")]
    public int TicketsCount { get; set; }
}
