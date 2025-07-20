namespace NotesApp.Domain.Exceptions;

public abstract class NotesAppException : Exception
{
    public string ErrorCode { get; }
    public DateTime Timestamp { get; }

    protected NotesAppException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }

    protected NotesAppException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }
}

public class ResourceNotFoundException : NotesAppException
{
    public string ResourceType { get; }
    public string Identifier { get; }

    public ResourceNotFoundException(string resourceType, string identifier)
        : base("ResourceNotFound", $"The {resourceType} with identifier {identifier} was not found.")
    {
        ResourceType = resourceType;
        Identifier = identifier;
    }
}

public class InvalidInputException : NotesAppException
{
    public InvalidInputException(string message)
        : base("InvalidInput", message) { }

    public InvalidInputException(string message, Exception innerException)
        : base("InvalidInput", message, innerException) { }
}

public class UnauthorizedAccessException : NotesAppException
{
    public UnauthorizedAccessException(string message)
        : base("UnauthorizedAccess", message) { }
}

public class ValidationException : NotesAppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("ValidationFailed", "One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly();
    }
}