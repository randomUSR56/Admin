using Admin.Models;

namespace Admin.Services;

public interface IApiClient
{
    // Auth
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<User> GetCurrentUserAsync();

    // Users
    Task<PaginatedResponse<User>> GetUsersAsync(int page = 1, string? role = null, string? search = null);
    Task<User> GetUserAsync(int id);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(int id, UpdateUserRequest request);
    Task DeleteUserAsync(int id);

    // Problems
    Task<PaginatedResponse<Problem>> GetProblemsAsync(int page = 1, string? category = null, bool? isActive = null, string? search = null);
    Task<Problem> GetProblemAsync(int id);
    Task<Problem> CreateProblemAsync(CreateProblemRequest request);
    Task UpdateProblemAsync(int id, UpdateProblemRequest request);
    Task DeleteProblemAsync(int id);
    Task<ProblemStatistics> GetProblemStatisticsAsync();

    // Health
    Task<bool> HealthCheckAsync();
}
