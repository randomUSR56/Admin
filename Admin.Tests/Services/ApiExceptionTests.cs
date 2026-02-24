using Admin.Services;

namespace Admin.Tests.Services;

public class ApiExceptionTests
{
    [Fact]
    public void ApiException_SetsMessage()
    {
        var ex = new ApiException("Test error", 500);
        Assert.Equal("Test error", ex.Message);
        Assert.Equal(500, ex.StatusCode);
    }

    [Fact]
    public void ApiException_IsUnauthorized_True_For401()
    {
        var ex = new ApiException("Unauthorized", 401);
        Assert.True(ex.IsUnauthorized);
        Assert.False(ex.IsForbidden);
        Assert.False(ex.IsValidationError);
    }

    [Fact]
    public void ApiException_IsForbidden_True_For403()
    {
        var ex = new ApiException("Forbidden", 403);
        Assert.False(ex.IsUnauthorized);
        Assert.True(ex.IsForbidden);
        Assert.False(ex.IsValidationError);
    }

    [Fact]
    public void ApiException_IsValidationError_True_For422()
    {
        var errors = new Dictionary<string, List<string>>
        {
            ["name"] = ["The name field is required."]
        };
        var ex = new ApiException("Validation failed", 422, errors);
        Assert.False(ex.IsUnauthorized);
        Assert.False(ex.IsForbidden);
        Assert.True(ex.IsValidationError);
        Assert.NotNull(ex.ValidationErrors);
        Assert.Single(ex.ValidationErrors);
    }

    [Fact]
    public void ApiException_ValidationErrors_Default_IsNull()
    {
        var ex = new ApiException("Error", 500);
        Assert.Null(ex.ValidationErrors);
    }

    [Fact]
    public void ApiException_StatusFlags_AllFalse_ForOtherCodes()
    {
        var ex = new ApiException("Server error", 500);
        Assert.False(ex.IsUnauthorized);
        Assert.False(ex.IsForbidden);
        Assert.False(ex.IsValidationError);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(404)]
    [InlineData(500)]
    public void ApiException_StatusCode_IsStoredCorrectly(int statusCode)
    {
        var ex = new ApiException("Test", statusCode);
        Assert.Equal(statusCode, ex.StatusCode);
    }
}
