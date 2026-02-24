using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class TicketRequestsTests
{
    [Fact]
    public void CreateTicketRequest_DefaultValues_AreCorrect()
    {
        var request = new CreateTicketRequest();
        Assert.Equal(0, request.CarId);
        Assert.Equal(string.Empty, request.Description);
        Assert.Equal("medium", request.Priority);
        Assert.Empty(request.ProblemIds);
        Assert.Empty(request.ProblemNotes);
    }

    [Fact]
    public void CreateTicketRequest_Serializes_ToSnakeCaseJson()
    {
        var request = new CreateTicketRequest
        {
            CarId = 3,
            Description = "Engine overheating",
            Priority = "high",
            ProblemIds = [1, 5, 7],
            ProblemNotes = ["Front left", null, "Rear brake"]
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(3, root.GetProperty("car_id").GetInt32());
        Assert.Equal("Engine overheating", root.GetProperty("description").GetString());
        Assert.Equal("high", root.GetProperty("priority").GetString());

        var problemIds = root.GetProperty("problem_ids");
        Assert.Equal(JsonValueKind.Array, problemIds.ValueKind);
        Assert.Equal(3, problemIds.GetArrayLength());
        Assert.Equal(1, problemIds[0].GetInt32());
        Assert.Equal(5, problemIds[1].GetInt32());
        Assert.Equal(7, problemIds[2].GetInt32());

        var problemNotes = root.GetProperty("problem_notes");
        Assert.Equal(JsonValueKind.Array, problemNotes.ValueKind);
        Assert.Equal(3, problemNotes.GetArrayLength());
        Assert.Equal("Front left", problemNotes[0].GetString());
        Assert.Equal(JsonValueKind.Null, problemNotes[1].ValueKind);
        Assert.Equal("Rear brake", problemNotes[2].GetString());
    }

    [Fact]
    public void CreateTicketRequest_Serializes_WithEmptyArrays()
    {
        var request = new CreateTicketRequest
        {
            CarId = 1,
            Description = "Check engine light",
            Priority = "low"
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(0, root.GetProperty("problem_ids").GetArrayLength());
        Assert.Equal(0, root.GetProperty("problem_notes").GetArrayLength());
    }

    [Fact]
    public void UpdateTicketRequest_DefaultValues_AreAllNull()
    {
        var request = new UpdateTicketRequest();
        Assert.Null(request.Description);
        Assert.Null(request.Priority);
        Assert.Null(request.Status);
        Assert.Null(request.MechanicId);
        Assert.Null(request.ProblemIds);
        Assert.Null(request.ProblemNotes);
    }

    [Fact]
    public void UpdateTicketRequest_Serializes_WithPartialFields()
    {
        var request = new UpdateTicketRequest
        {
            Priority = "urgent",
            MechanicId = 10
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("urgent", root.GetProperty("priority").GetString());
        Assert.Equal(10, root.GetProperty("mechanic_id").GetInt32());
    }

    [Fact]
    public void UpdateTicketRequest_Serializes_AllFields()
    {
        var request = new UpdateTicketRequest
        {
            Description = "Updated description",
            Priority = "high",
            Status = "assigned",
            MechanicId = 5,
            ProblemIds = [2, 4],
            ProblemNotes = ["Note A", "Note B"]
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Updated description", root.GetProperty("description").GetString());
        Assert.Equal("high", root.GetProperty("priority").GetString());
        Assert.Equal("assigned", root.GetProperty("status").GetString());
        Assert.Equal(5, root.GetProperty("mechanic_id").GetInt32());
        Assert.Equal(2, root.GetProperty("problem_ids").GetArrayLength());
        Assert.Equal(2, root.GetProperty("problem_notes").GetArrayLength());
    }

    [Fact]
    public void UpdateTicketRequest_Serializes_WithProblemIdsAndNotes()
    {
        var request = new UpdateTicketRequest
        {
            ProblemIds = [1, 3, 5],
            ProblemNotes = ["Note 1", null, "Note 3"]
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var ids = root.GetProperty("problem_ids");
        Assert.Equal(3, ids.GetArrayLength());
        Assert.Equal(1, ids[0].GetInt32());
        Assert.Equal(3, ids[1].GetInt32());
        Assert.Equal(5, ids[2].GetInt32());

        var notes = root.GetProperty("problem_notes");
        Assert.Equal(3, notes.GetArrayLength());
        Assert.Equal("Note 1", notes[0].GetString());
        Assert.Equal(JsonValueKind.Null, notes[1].ValueKind);
        Assert.Equal("Note 3", notes[2].GetString());
    }
}
