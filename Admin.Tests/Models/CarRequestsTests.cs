using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class CarRequestsTests
{
    [Fact]
    public void CreateCarRequest_DefaultValues_AreCorrect()
    {
        var request = new CreateCarRequest();
        Assert.Equal(0, request.UserId);
        Assert.Equal(string.Empty, request.Make);
        Assert.Equal(string.Empty, request.Model);
        Assert.Equal(0, request.Year);
        Assert.Equal(string.Empty, request.LicensePlate);
        Assert.Null(request.Vin);
        Assert.Null(request.Color);
    }

    [Fact]
    public void CreateCarRequest_Serializes_ToSnakeCaseJson()
    {
        var request = new CreateCarRequest
        {
            UserId = 3,
            Make = "Toyota",
            Model = "Corolla",
            Year = 2024,
            LicensePlate = "ABC-123",
            Vin = "1234567890ABCDEFG",
            Color = "Red"
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(3, root.GetProperty("user_id").GetInt32());
        Assert.Equal("Toyota", root.GetProperty("make").GetString());
        Assert.Equal("Corolla", root.GetProperty("model").GetString());
        Assert.Equal(2024, root.GetProperty("year").GetInt32());
        Assert.Equal("ABC-123", root.GetProperty("license_plate").GetString());
        Assert.Equal("1234567890ABCDEFG", root.GetProperty("vin").GetString());
        Assert.Equal("Red", root.GetProperty("color").GetString());
    }

    [Fact]
    public void CreateCarRequest_Serializes_WithNullOptionals()
    {
        var request = new CreateCarRequest
        {
            UserId = 1,
            Make = "Ford",
            Model = "Focus",
            Year = 2020,
            LicensePlate = "XYZ-999"
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("vin").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("color").ValueKind);
    }

    [Fact]
    public void UpdateCarRequest_DefaultValues_AreAllNull()
    {
        var request = new UpdateCarRequest();
        Assert.Null(request.UserId);
        Assert.Null(request.Make);
        Assert.Null(request.Model);
        Assert.Null(request.Year);
        Assert.Null(request.LicensePlate);
        Assert.Null(request.Vin);
        Assert.Null(request.Color);
    }

    [Fact]
    public void UpdateCarRequest_Serializes_WithPartialFields()
    {
        var request = new UpdateCarRequest
        {
            Make = "Honda",
            Color = "Blue"
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Honda", root.GetProperty("make").GetString());
        Assert.Equal("Blue", root.GetProperty("color").GetString());
    }

    [Fact]
    public void UpdateCarRequest_Serializes_AllFields()
    {
        var request = new UpdateCarRequest
        {
            UserId = 5,
            Make = "Mercedes",
            Model = "C-Class",
            Year = 2025,
            LicensePlate = "NEW-001",
            Vin = "WDDWF8DB1FA123456",
            Color = "Silver"
        };

        var json = JsonSerializer.Serialize(request);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(5, root.GetProperty("user_id").GetInt32());
        Assert.Equal("Mercedes", root.GetProperty("make").GetString());
        Assert.Equal("C-Class", root.GetProperty("model").GetString());
        Assert.Equal(2025, root.GetProperty("year").GetInt32());
        Assert.Equal("NEW-001", root.GetProperty("license_plate").GetString());
        Assert.Equal("WDDWF8DB1FA123456", root.GetProperty("vin").GetString());
        Assert.Equal("Silver", root.GetProperty("color").GetString());
    }
}
