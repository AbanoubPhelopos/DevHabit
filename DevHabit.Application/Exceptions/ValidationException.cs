namespace DevHabit.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public Dictionary<string, object> InvalidProperties { get; }

    public ValidationException(string message) : base(message)
    {
        InvalidProperties = new Dictionary<string, object>();
    }

    public ValidationException(string message, Dictionary<string, object> invalidProperties)
        : base(message)
    {
        InvalidProperties = invalidProperties;
    }
}
