using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class ProblemRequestsTests
{
    [Fact]
    public void CreateProblemRequest_DefaultValues_AreCorrect()
    {
        var request = new CreateProblemRequest();
        Assert.Equal(string.Empty, request.Name);
        Assert.Equal(string.Empty, request.Category);
        Assert.Null(request.Description);
        Assert.True(request.IsActive);
    }

    [Fact]
    public void CreateProblemRequest_Serializes_ToSnakeCaseJson()
    {
        var request = new CreateProblemRequest
        {
            Name = "Worn brake pads",
            Category = "brakes",
            Description = "Front or rear brake pads below minimum thickness.",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Worn brake pads", root.GetProperty("name").GetString());
        Assert.Equal("brakes", root.GetProperty("category").GetString());
        Assert.Equal("Front or rear brake pads below minimum thickness.", root.GetProperty("description").GetString());
        Assert.True(root.GetProperty("is_active").GetBoolean());
    }

    [Fact]
    public void UpdateProblemRequest_DefaultValues_AreAllNull()
    {
        var request = new UpdateProblemRequest();
        Assert.Null(request.Name);
        Assert.Null(request.Category);
        Assert.Null(request.Description);
        Assert.Null(request.IsActive);
    }

    [Fact]
    public void UpdateProblemRequest_Serializes_WithPartialFields()
    {
        var request = new UpdateProblemRequest
        {
            Name = "Updated name",
            IsActive = false
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Updated name", root.GetProperty("name").GetString());
        Assert.False(root.GetProperty("is_active").GetBoolean());
    }
}
