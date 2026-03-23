namespace DevHabit.Application.Results;

public sealed class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; private set; }
    public int StatusCode { get; private set; }

    private ServiceResult() { }

    public static ServiceResult Success(string? message = null, int statusCode = 200)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ServiceResult Failure(string message, int statusCode = 400)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode
        };
    }
}

public sealed class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; private set; }
    public int StatusCode { get; private set; }
    public T? Data { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> SuccessData(T data, string? message = null, int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> SuccessWithCount(T data, int count, string? message = null, int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message ?? $"Count: {count}",
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> Failure(string message, int statusCode = 400)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode
        };
    }
}
