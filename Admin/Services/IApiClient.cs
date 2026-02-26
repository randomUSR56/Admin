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

    // Cars
    Task<PaginatedResponse<Car>> GetCarsAsync(int page = 1, int? userId = null, string? search = null);
    Task<Car> GetCarAsync(int id);
    Task<Car> CreateCarAsync(CreateCarRequest request);
    Task UpdateCarAsync(int id, UpdateCarRequest request);
    Task DeleteCarAsync(int id);

    // Tickets
    Task<PaginatedResponse<Ticket>> GetTicketsAsync(int page = 1, string? status = null, string? priority = null, int? mechanicId = null, int? userId = null, int? carId = null);
    Task<Ticket> GetTicketAsync(int id);
    Task<Ticket> CreateTicketAsync(CreateTicketRequest request);
    Task UpdateTicketAsync(int id, UpdateTicketRequest request);
    Task DeleteTicketAsync(int id);
    Task<Ticket> AcceptTicketAsync(int id);
    Task<Ticket> StartTicketAsync(int id);
    Task<Ticket> CompleteTicketAsync(int id);
    Task<Ticket> CloseTicketAsync(int id);
    Task<TicketStatistics> GetTicketStatisticsAsync();

    // Health
    Task<bool> HealthCheckAsync();
}
