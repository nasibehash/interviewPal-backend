namespace InterviewPal.Application.Services;

public class NotFoundException(string message) : Exception(message);

public class RequestValidationException(string message) : Exception(message);
