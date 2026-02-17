using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Admin.Models;

namespace Admin.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthTokenStore _tokenStore;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient, AuthTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // --- Auth ---

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            throw new ApiException(
                error?.Message ?? "Login failed",
                (int)response.StatusCode,
                error?.Errors);
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions)
            ?? throw new ApiException("Invalid response from server", (int)response.StatusCode);

        await _tokenStore.SaveTokenAsync(result.Token);

        if (result.User is not null)
            await _tokenStore.SaveUserInfoAsync(result.User.Id, result.User.Name, result.User.Email);

        return result;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            await _httpClient.PostAsync("/api/logout", null);
        }
        finally
        {
            _tokenStore.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<User> GetCurrentUserAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("/api/user");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions)
            ?? throw new ApiException("Invalid response", (int)response.StatusCode);
    }

    // --- Users ---

    public async Task<PaginatedResponse<User>> GetUsersAsync(int page = 1, string? role = null, string? search = null)
    {
        await SetAuthHeaderAsync();

        var query = $"/api/users?page={page}";
        if (!string.IsNullOrEmpty(role))
            query += $"&role={Uri.EscapeDataString(role)}";
        if (!string.IsNullOrEmpty(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query);
        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<PaginatedResponse<User>>(JsonOptions)
            ?? throw new ApiException("Invalid response", (int)response.StatusCode);
    }

    public async Task<User> GetUserAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"/api/users/{id}");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions)
            ?? throw new ApiException("Invalid response", (int)response.StatusCode);
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("/api/users", request);
        await EnsureSuccessAsync(response);

        var wrapper = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        if (wrapper.TryGetProperty("data", out var dataElement))
        {
            return JsonSerializer.Deserialize<User>(dataElement.GetRawText(), JsonOptions)
                ?? throw new ApiException("Invalid response", (int)response.StatusCode);
        }

        return JsonSerializer.Deserialize<User>(wrapper.GetRawText(), JsonOptions)
            ?? throw new ApiException("Invalid response", (int)response.StatusCode);
    }

    public async Task UpdateUserAsync(int id, UpdateUserRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"/api/users/{id}", request);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteUserAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"/api/users/{id}");
        await EnsureSuccessAsync(response);
    }

    // --- Health ---

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // --- Helpers ---

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        throw new ApiException(
            error?.Message ?? $"Request failed with status {(int)response.StatusCode}",
            (int)response.StatusCode,
            error?.Errors);
    }
}

public class ApiException : Exception
{
    public int StatusCode { get; }
    public Dictionary<string, List<string>>? ValidationErrors { get; }

    public ApiException(string message, int statusCode, Dictionary<string, List<string>>? validationErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ValidationErrors = validationErrors;
    }

    public bool IsUnauthorized => StatusCode == 401;
    public bool IsForbidden => StatusCode == 403;
    public bool IsValidationError => StatusCode == 422;
}
