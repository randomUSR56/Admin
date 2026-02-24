using Admin.Models;
using Admin.Services;
using Moq;

namespace Admin.Tests.ViewModels;

/// <summary>
/// Tests for the Cars API client methods via the IApiClient interface.
/// These tests validate the contract and mock behavior patterns used by the ViewModels.
/// </summary>
public class CarsApiClientTests
{
    private readonly Mock<IApiClient> _mockApiClient;

    public CarsApiClientTests()
    {
        _mockApiClient = new Mock<IApiClient>();
    }

    [Fact]
    public async Task GetCarsAsync_ReturnsEmptyPaginatedResponse()
    {
        var expected = new PaginatedResponse<Car>
        {
            Data = [],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 0
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(1, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync();

        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task GetCarsAsync_ReturnsPaginatedCars()
    {
        var cars = new List<Car>
        {
            new() { Id = 1, UserId = 1, Make = "Toyota", Model = "Corolla", Year = 2024, LicensePlate = "ABC-123" },
            new() { Id = 2, UserId = 2, Make = "BMW", Model = "X5", Year = 2023, LicensePlate = "DEF-456" },
            new() { Id = 3, UserId = 1, Make = "Ford", Model = "Focus", Year = 2020, LicensePlate = "GHI-789" }
        };

        var expected = new PaginatedResponse<Car>
        {
            Data = cars,
            CurrentPage = 1,
            LastPage = 2,
            PerPage = 15,
            Total = 20
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(1, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync();

        Assert.Equal(3, result.Data.Count);
        Assert.Equal(2, result.LastPage);
        Assert.Equal(20, result.Total);
    }

    [Fact]
    public async Task GetCarsAsync_WithUserIdFilter()
    {
        var expected = new PaginatedResponse<Car>
        {
            Data = [new() { Id = 1, UserId = 3, Make = "Toyota", Model = "Corolla", Year = 2024, LicensePlate = "ABC-123" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(1, 3, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync(1, 3);

        Assert.Single(result.Data);
        Assert.Equal(3, result.Data[0].UserId);
    }

    [Fact]
    public async Task GetCarsAsync_WithSearchFilter()
    {
        var expected = new PaginatedResponse<Car>
        {
            Data = [new() { Id = 1, UserId = 1, Make = "Toyota", Model = "Corolla", Year = 2024, LicensePlate = "ABC-123" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(1, null, "Toyota"))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync(1, null, "Toyota");

        Assert.Single(result.Data);
        Assert.Contains("Toyota", result.Data[0].Make, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCarAsync_ReturnsCar()
    {
        var expected = new Car
        {
            Id = 7,
            UserId = 3,
            Make = "BMW",
            Model = "X5",
            Year = 2023,
            LicensePlate = "ABC-123",
            Vin = "WBA1234567890",
            Color = "Black"
        };

        _mockApiClient
            .Setup(c => c.GetCarAsync(7))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarAsync(7);

        Assert.Equal(7, result.Id);
        Assert.Equal("BMW", result.Make);
        Assert.Equal("X5", result.Model);
        Assert.Equal(2023, result.Year);
        Assert.Equal("WBA1234567890", result.Vin);
    }

    [Fact]
    public async Task GetCarAsync_ThrowsApiException_ForNotFound()
    {
        _mockApiClient
            .Setup(c => c.GetCarAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.GetCarAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task CreateCarAsync_ReturnsCar()
    {
        var request = new CreateCarRequest
        {
            UserId = 1,
            Make = "Tesla",
            Model = "Model 3",
            Year = 2024,
            LicensePlate = "EV-001",
            Color = "White"
        };

        var expected = new Car
        {
            Id = 10,
            UserId = 1,
            Make = "Tesla",
            Model = "Model 3",
            Year = 2024,
            LicensePlate = "EV-001",
            Color = "White"
        };

        _mockApiClient
            .Setup(c => c.CreateCarAsync(request))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.CreateCarAsync(request);

        Assert.Equal(10, result.Id);
        Assert.Equal("Tesla", result.Make);
        Assert.Equal("Model 3", result.Model);
    }

    [Fact]
    public async Task CreateCarAsync_ThrowsOnValidationError()
    {
        var request = new CreateCarRequest { UserId = 0, Make = "", Model = "", Year = 0, LicensePlate = "" };
        var errors = new Dictionary<string, List<string>>
        {
            ["user_id"] = ["The user id field is required."],
            ["make"] = ["The make field is required."],
            ["model"] = ["The model field is required."],
            ["license_plate"] = ["The license plate field is required."]
        };

        _mockApiClient
            .Setup(c => c.CreateCarAsync(request))
            .ThrowsAsync(new ApiException("Validation failed", 422, errors));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.CreateCarAsync(request));

        Assert.True(ex.IsValidationError);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Equal(4, ex.ValidationErrors.Count);
    }

    [Fact]
    public async Task UpdateCarAsync_Succeeds()
    {
        var request = new UpdateCarRequest { Color = "Red" };

        _mockApiClient
            .Setup(c => c.UpdateCarAsync(7, request))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.UpdateCarAsync(7, request);

        _mockApiClient.Verify(c => c.UpdateCarAsync(7, request), Times.Once);
    }

    [Fact]
    public async Task DeleteCarAsync_Succeeds()
    {
        _mockApiClient
            .Setup(c => c.DeleteCarAsync(7))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.DeleteCarAsync(7);

        _mockApiClient.Verify(c => c.DeleteCarAsync(7), Times.Once);
    }

    [Fact]
    public async Task DeleteCarAsync_ThrowsOnNotFound()
    {
        _mockApiClient
            .Setup(c => c.DeleteCarAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.DeleteCarAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task GetCarsAsync_Page2_ReturnsCorrectPage()
    {
        var expected = new PaginatedResponse<Car>
        {
            Data = [new() { Id = 16, UserId = 1, Make = "Audi", Model = "A4", Year = 2022, LicensePlate = "AUD-016" }],
            CurrentPage = 2,
            LastPage = 3,
            PerPage = 15,
            Total = 40
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(2, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync(2);

        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.LastPage);
    }

    [Fact]
    public async Task GetCarsAsync_WithUserIdAndSearch()
    {
        var expected = new PaginatedResponse<Car>
        {
            Data = [new() { Id = 5, UserId = 2, Make = "Toyota", Model = "Camry", Year = 2021, LicensePlate = "TOY-005" }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetCarsAsync(1, 2, "Toyota"))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarsAsync(1, 2, "Toyota");

        Assert.Single(result.Data);
        Assert.Equal(2, result.Data[0].UserId);
        Assert.Equal("Toyota", result.Data[0].Make);
    }

    [Fact]
    public async Task UpdateCarAsync_ThrowsOnUnauthorized()
    {
        var request = new UpdateCarRequest { Make = "Honda" };

        _mockApiClient
            .Setup(c => c.UpdateCarAsync(1, request))
            .ThrowsAsync(new ApiException("Unauthorized", 401));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.UpdateCarAsync(1, request));

        Assert.True(ex.IsUnauthorized);
    }

    [Fact]
    public async Task GetCarAsync_ReturnsCarWithUser()
    {
        var expected = new Car
        {
            Id = 5,
            UserId = 2,
            Make = "Tesla",
            Model = "Model 3",
            Year = 2024,
            LicensePlate = "EV-001",
            User = new User { Id = 2, Name = "John Doe", Email = "john@example.com" }
        };

        _mockApiClient
            .Setup(c => c.GetCarAsync(5))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetCarAsync(5);

        Assert.NotNull(result.User);
        Assert.Equal("John Doe", result.User.Name);
    }
}
