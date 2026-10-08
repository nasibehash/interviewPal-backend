namespace InterviewPal.Application.Services;

public class NotFoundException(string message) : Exception(message);

public class RequestValidationException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class UnauthorizedException(string message) : Exception(message);

public class TooManyRequestsException(string message) : Exception(message);
