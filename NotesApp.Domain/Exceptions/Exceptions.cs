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

public class DuplicateResourceException : NotesAppException
{
    public string ResourceType { get; }
    public string Identifier { get; }

    public DuplicateResourceException(string resourceType, string identifier)
        : base("DuplicateResource", $"The {resourceType} with identifier {identifier} already exists.")
    {
        ResourceType = resourceType;
        Identifier = identifier;
    }
}

public class ResourceInUseException : NotesAppException
{
    public string ResourceType { get; }
    public string Identifier { get; }
    public int AssociatedCount { get; }

    public ResourceInUseException(string resourceType, string identifier, int associatedCount)
        : base("ResourceInUse", $"Cannot delete {resourceType} with identifier {identifier} because it is associated with {associatedCount} resource(s).")
    {
        ResourceType = resourceType;
        Identifier = identifier;
        AssociatedCount = associatedCount;
    }
}

public class ConfigurationException : NotesAppException
{
    public ConfigurationException(string message)
        : base("ConfigurationError", message)
    {
    }
}

public class FileOperationException : NotesAppException
{
    public string OperationType { get; }
    public string FileName { get; }

    public FileOperationException(string operationType, string fileName, string message, Exception? innerException = null)
        : base("FileOperationError", message, innerException)
    {
        OperationType = operationType;
        FileName = fileName;
    }
}