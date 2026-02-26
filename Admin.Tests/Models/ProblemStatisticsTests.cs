using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class ProblemStatisticsTests
{
    [Fact]
    public void ProblemStatistics_DefaultValues_AreCorrect()
    {
        var stats = new ProblemStatistics();
        Assert.Equal(0, stats.TotalProblems);
        Assert.Equal(0, stats.ActiveProblems);
        Assert.Empty(stats.ProblemsByFrequency);
    }

    [Fact]
    public void ProblemStatistics_Deserializes_FromJson()
    {
        var json = """
        {
            "total_problems": 24,
            "active_problems": 21,
            "problems_by_frequency": [
                {
                    "id": 3,
                    "name": "Worn brake pads",
                    "category": "brakes",
                    "tickets_count": 42
                },
                {
                    "id": 7,
                    "name": "Engine overheating",
                    "category": "engine",
                    "tickets_count": 18
                }
            ]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stats = JsonSerializer.Deserialize<ProblemStatistics>(json, options);

        Assert.NotNull(stats);
        Assert.Equal(24, stats.TotalProblems);
        Assert.Equal(21, stats.ActiveProblems);
        Assert.Equal(2, stats.ProblemsByFrequency.Count);
        Assert.Equal("Worn brake pads", stats.ProblemsByFrequency[0].Name);
        Assert.Equal(42, stats.ProblemsByFrequency[0].TicketsCount);
        Assert.Equal("engine", stats.ProblemsByFrequency[1].Category);
    }

    [Fact]
    public void ProblemFrequency_DefaultValues_AreCorrect()
    {
        var freq = new ProblemFrequency();
        Assert.Equal(0, freq.Id);
        Assert.Equal(string.Empty, freq.Name);
        Assert.Equal(string.Empty, freq.Category);
        Assert.Equal(0, freq.TicketsCount);
    }
}
