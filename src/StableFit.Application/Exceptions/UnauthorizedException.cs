namespace StableFit.Application.Exceptions;

public sealed class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base(message, 401, "UNAUTHORIZED")
    {
    }
}
