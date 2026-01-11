using System;

namespace AuthApp.Common.Exeptions;

public abstract class DomainException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
