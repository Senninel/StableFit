namespace StableFit.Application.Exceptions;

/// <summary>Base for all application-level exceptions that map to HTTP responses.</summary>
public abstract class BaseException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected BaseException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
