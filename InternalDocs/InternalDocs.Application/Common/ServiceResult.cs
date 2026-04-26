namespace InternalDocs.Application.Common;

public sealed record ServiceResult(
    bool Succeeded,
    string? Error,
    ServiceErrorType? ErrorType)
{
    public static ServiceResult Success()
    {
        return new ServiceResult(true, null, null);
    }

    public static ServiceResult Failure(string error, ServiceErrorType errorType)
    {
        return new ServiceResult(false, error, errorType);
    }
}
