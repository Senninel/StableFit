namespace StableFit.Application.Exceptions;

public sealed class NotFoundException : BaseException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.", 404, "NOT_FOUND")
    {
    }
}
