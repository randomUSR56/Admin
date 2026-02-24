using Admin.Models;
using Admin.Services;
using Moq;

namespace Admin.Tests.ViewModels;

/// <summary>
/// Tests for the Problems API client methods via the IApiClient interface.
/// These tests validate the contract and mock behavior patterns used by the ViewModels.
/// </summary>
public class ProblemsApiClientTests
{
    private readonly Mock<IApiClient> _mockApiClient;

    public ProblemsApiClientTests()
    {
        _mockApiClient = new Mock<IApiClient>();
    }

    [Fact]
    public async Task GetProblemsAsync_ReturnsEmptyPaginatedResponse()
    {
        var expected = new PaginatedResponse<Problem>
        {
            Data = [],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 0
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(1, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync();

        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task GetProblemsAsync_ReturnsPaginatedProblems()
    {
        var problems = new List<Problem>
        {
            new() { Id = 1, Name = "Worn brake pads", Category = "brakes", IsActive = true },
            new() { Id = 2, Name = "Engine overheating", Category = "engine", IsActive = true },
            new() { Id = 3, Name = "Faulty wiring", Category = "electrical", IsActive = false }
        };

        var expected = new PaginatedResponse<Problem>
        {
            Data = problems,
            CurrentPage = 1,
            LastPage = 2,
            PerPage = 15,
            Total = 20
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(1, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync();

        Assert.Equal(3, result.Data.Count);
        Assert.Equal(2, result.LastPage);
        Assert.Equal(20, result.Total);
    }

    [Fact]
    public async Task GetProblemsAsync_WithCategoryFilter()
    {
        var expected = new PaginatedResponse<Problem>
        {
            Data = [new() { Id = 1, Name = "Worn brake pads", Category = "brakes", IsActive = true }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(1, "brakes", null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync(1, "brakes");

        Assert.Single(result.Data);
        Assert.Equal("brakes", result.Data[0].Category);
    }

    [Fact]
    public async Task GetProblemsAsync_WithActiveFilter()
    {
        var expected = new PaginatedResponse<Problem>
        {
            Data = [new() { Id = 1, Name = "Test", Category = "other", IsActive = true }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(1, null, true, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync(1, null, true);

        Assert.Single(result.Data);
        Assert.True(result.Data[0].IsActive);
    }

    [Fact]
    public async Task GetProblemsAsync_WithSearchFilter()
    {
        var expected = new PaginatedResponse<Problem>
        {
            Data = [new() { Id = 1, Name = "Worn brake pads", Category = "brakes", IsActive = true }],
            CurrentPage = 1,
            LastPage = 1,
            PerPage = 15,
            Total = 1
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(1, null, null, "brake"))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync(1, null, null, "brake");

        Assert.Single(result.Data);
        Assert.Contains("brake", result.Data[0].Name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetProblemAsync_ReturnsProblem()
    {
        var expected = new Problem
        {
            Id = 5,
            Name = "Worn brake pads",
            Category = "brakes",
            Description = "Below minimum thickness",
            IsActive = true
        };

        _mockApiClient
            .Setup(c => c.GetProblemAsync(5))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemAsync(5);

        Assert.Equal(5, result.Id);
        Assert.Equal("Worn brake pads", result.Name);
        Assert.Equal("brakes", result.Category);
    }

    [Fact]
    public async Task GetProblemAsync_ThrowsApiException_ForNotFound()
    {
        _mockApiClient
            .Setup(c => c.GetProblemAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.GetProblemAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task CreateProblemAsync_ReturnsProblem()
    {
        var request = new CreateProblemRequest
        {
            Name = "New problem",
            Category = "engine",
            Description = "Test description",
            IsActive = true
        };

        var expected = new Problem
        {
            Id = 10,
            Name = "New problem",
            Category = "engine",
            Description = "Test description",
            IsActive = true
        };

        _mockApiClient
            .Setup(c => c.CreateProblemAsync(request))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.CreateProblemAsync(request);

        Assert.Equal(10, result.Id);
        Assert.Equal("New problem", result.Name);
    }

    [Fact]
    public async Task CreateProblemAsync_ThrowsOnValidationError()
    {
        var request = new CreateProblemRequest { Name = "", Category = "" };
        var errors = new Dictionary<string, List<string>>
        {
            ["name"] = ["The name field is required."],
            ["category"] = ["The category field is required."]
        };

        _mockApiClient
            .Setup(c => c.CreateProblemAsync(request))
            .ThrowsAsync(new ApiException("Validation failed", 422, errors));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.CreateProblemAsync(request));

        Assert.True(ex.IsValidationError);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Equal(2, ex.ValidationErrors.Count);
    }

    [Fact]
    public async Task UpdateProblemAsync_Succeeds()
    {
        var request = new UpdateProblemRequest { Name = "Updated name" };

        _mockApiClient
            .Setup(c => c.UpdateProblemAsync(5, request))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.UpdateProblemAsync(5, request);

        _mockApiClient.Verify(c => c.UpdateProblemAsync(5, request), Times.Once);
    }

    [Fact]
    public async Task DeleteProblemAsync_Succeeds()
    {
        _mockApiClient
            .Setup(c => c.DeleteProblemAsync(5))
            .Returns(Task.CompletedTask);

        await _mockApiClient.Object.DeleteProblemAsync(5);

        _mockApiClient.Verify(c => c.DeleteProblemAsync(5), Times.Once);
    }

    [Fact]
    public async Task DeleteProblemAsync_ThrowsOnNotFound()
    {
        _mockApiClient
            .Setup(c => c.DeleteProblemAsync(999))
            .ThrowsAsync(new ApiException("Not found", 404));

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _mockApiClient.Object.DeleteProblemAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task GetProblemStatisticsAsync_ReturnsStatistics()
    {
        var expected = new ProblemStatistics
        {
            TotalProblems = 24,
            ActiveProblems = 21,
            ProblemsByFrequency =
            [
                new() { Id = 3, Name = "Worn brake pads", Category = "brakes", TicketsCount = 42 }
            ]
        };

        _mockApiClient
            .Setup(c => c.GetProblemStatisticsAsync())
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemStatisticsAsync();

        Assert.Equal(24, result.TotalProblems);
        Assert.Equal(21, result.ActiveProblems);
        Assert.Single(result.ProblemsByFrequency);
    }

    [Fact]
    public async Task GetProblemsAsync_Page2_ReturnsCorrectPage()
    {
        var expected = new PaginatedResponse<Problem>
        {
            Data = [new() { Id = 16, Name = "Test", Category = "other", IsActive = true }],
            CurrentPage = 2,
            LastPage = 3,
            PerPage = 15,
            Total = 40
        };

        _mockApiClient
            .Setup(c => c.GetProblemsAsync(2, null, null, null))
            .ReturnsAsync(expected);

        var result = await _mockApiClient.Object.GetProblemsAsync(2);

        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.LastPage);
    }
}
