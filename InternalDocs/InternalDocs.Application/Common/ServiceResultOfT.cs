namespace InternalDocs.Application.Common;

public sealed record ServiceResult<T>(
    bool Succeeded,
    T? Value,
    string? Error,
    ServiceErrorType? ErrorType)
{
    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(true, value, null, null);
    }

    public static ServiceResult<T> Failure(string error, ServiceErrorType errorType)
    {
        return new ServiceResult<T>(false, default, error, errorType);
    }
}
