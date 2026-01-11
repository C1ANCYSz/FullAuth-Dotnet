
namespace AuthApp.Common;

public abstract class DomainException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

