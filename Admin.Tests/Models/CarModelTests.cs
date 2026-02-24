using System.Text.Json;
using Admin.Models;

namespace Admin.Tests.Models;

public class CarModelTests
{
    [Fact]
    public void Car_DisplayName_FormatsCorrectly()
    {
        var car = new Car { Year = 2024, Make = "Toyota", Model = "Corolla" };
        Assert.Equal("2024 Toyota Corolla", car.DisplayName);
    }

    [Fact]
    public void Car_DefaultValues_AreCorrect()
    {
        var car = new Car();
        Assert.Equal(0, car.Id);
        Assert.Equal(0, car.UserId);
        Assert.Equal(string.Empty, car.Make);
        Assert.Equal(string.Empty, car.Model);
        Assert.Equal(0, car.Year);
        Assert.Equal(string.Empty, car.LicensePlate);
        Assert.Null(car.Vin);
        Assert.Null(car.Color);
        Assert.Null(car.CreatedAt);
        Assert.Null(car.UpdatedAt);
        Assert.Null(car.User);
    }

    [Fact]
    public void Car_Deserializes_FromSnakeCaseJson()
    {
        var json = """
        {
            "id": 7,
            "user_id": 3,
            "make": "BMW",
            "model": "X5",
            "year": 2023,
            "license_plate": "ABC-123",
            "vin": "WBA1234567890",
            "color": "Black",
            "created_at": "2025-06-01T10:00:00.000000Z",
            "updated_at": "2025-06-02T12:00:00.000000Z"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var car = JsonSerializer.Deserialize<Car>(json, options);

        Assert.NotNull(car);
        Assert.Equal(7, car.Id);
        Assert.Equal(3, car.UserId);
        Assert.Equal("BMW", car.Make);
        Assert.Equal("X5", car.Model);
        Assert.Equal(2023, car.Year);
        Assert.Equal("ABC-123", car.LicensePlate);
        Assert.Equal("WBA1234567890", car.Vin);
        Assert.Equal("Black", car.Color);
        Assert.Equal("2023 BMW X5", car.DisplayName);
    }

    [Fact]
    public void Car_Deserializes_WithNullOptionalFields()
    {
        var json = """
        {
            "id": 1,
            "user_id": 1,
            "make": "Ford",
            "model": "Focus",
            "year": 2020,
            "license_plate": "XYZ-999",
            "vin": null,
            "color": null
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var car = JsonSerializer.Deserialize<Car>(json, options);

        Assert.NotNull(car);
        Assert.Null(car.Vin);
        Assert.Null(car.Color);
    }

    [Fact]
    public void Car_Deserializes_WithNestedUser()
    {
        var json = """
        {
            "id": 5,
            "user_id": 2,
            "make": "Tesla",
            "model": "Model 3",
            "year": 2024,
            "license_plate": "EV-001",
            "user": {
                "id": 2,
                "name": "John Doe",
                "email": "john@example.com"
            }
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var car = JsonSerializer.Deserialize<Car>(json, options);

        Assert.NotNull(car);
        Assert.NotNull(car.User);
        Assert.Equal(2, car.User.Id);
        Assert.Equal("John Doe", car.User.Name);
        Assert.Equal("john@example.com", car.User.Email);
    }

    [Fact]
    public void Car_DisplayName_HandlesEmptyStrings()
    {
        var car = new Car { Year = 0, Make = "", Model = "" };
        Assert.Equal("0  ", car.DisplayName);
    }
}
