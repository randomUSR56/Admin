namespace Admin.Services;

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
