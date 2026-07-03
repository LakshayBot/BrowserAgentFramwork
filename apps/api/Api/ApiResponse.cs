namespace BrowserAgent.Api.Api;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<ApiError> Errors { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;

    public static ApiResponse<T> Ok(T data, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static ApiResponse<T> Fail(IEnumerable<ApiError> errors, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors.ToList(),
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static ApiResponse<T> Fail(string code, string message, string? correlationId = null)
    {
        return Fail(new[] { new ApiError { Code = code, Message = message } }, correlationId);
    }
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
}
