namespace AuthApp.Common.Errors;

public sealed class ConflictException(string message)
    : DomainException(message, StatusCodes.Status409Conflict) { }

public sealed class NotFoundException(string message)
    : DomainException(message, StatusCodes.Status404NotFound) { }

public sealed class BadRequestException(string message)
    : DomainException(message, StatusCodes.Status400BadRequest) { }

public sealed class UnauthorizedException(string message)
    : DomainException(message, StatusCodes.Status401Unauthorized) { }

public sealed class ForbiddenException(string message)
    : DomainException(message, StatusCodes.Status403Forbidden) { }

public sealed class GoneException(string message)
    : DomainException(message, StatusCodes.Status410Gone) { }

public sealed class UnprocessableEntityException(string message)
    : DomainException(message, StatusCodes.Status422UnprocessableEntity) { }

public sealed class TooManyRequestsException(string message)
    : DomainException(message, StatusCodes.Status429TooManyRequests) { }

public sealed class InvalidCredentialsException(string message)
    : DomainException(message, StatusCodes.Status401Unauthorized) { }

public sealed class TokenExpiredException(string message)
    : DomainException(message, StatusCodes.Status401Unauthorized) { }

public sealed class TokenRevokedException(string message)
    : DomainException(message, StatusCodes.Status401Unauthorized) { }

public sealed class InvalidTokenException(string message)
    : DomainException(message, StatusCodes.Status401Unauthorized) { }
