namespace BueiroInteligente.Core
{
    public abstract class BueiroInteligenteException : Exception
    {
        protected BueiroInteligenteException(string message)
            : base(message) { }

        protected BueiroInteligenteException(string message, Exception? innerException)
            : base(message, innerException) { }
    }

    public sealed class LogicException : BueiroInteligenteException
    {
        public LogicException(string message)
            : base(message) { }

        public LogicException(string message, Exception? innerException)
            : base(message, innerException) { }

        public static LogicException NullValue(string parameterName)
        {
            return new LogicException($"The value for '{parameterName}' cannot be null.");
        }

        public static LogicException InvalidValue(string parameterName, object? value)
        {
            var valueText = value?.ToString() ?? "null";
            return new LogicException($"Invalid value for '{parameterName}': '{valueText}'.");
        }
    }

    public sealed class ExternalApiException : BueiroInteligenteException
    {
        public ExternalApiException(string message)
            : base(message) { }

        public ExternalApiException(string message, Exception? innerException)
            : base(message, innerException) { }

        public ExternalApiException(
            string apiName,
            string message,
            Exception? innerException = null
        )
            : base($"External API '{apiName}': {message}", innerException)
        {
            ApiName = apiName;
        }

        public string? ApiName { get; }
    }

    public sealed class ConnectionException : BueiroInteligenteException
    {
        public ConnectionException(string message)
            : base(message) { }

        public ConnectionException(string message, Exception? innerException)
            : base(message, innerException) { }

        public ConnectionException(
            string resourceName,
            string message,
            Exception? innerException = null
        )
            : base($"Connection failure in '{resourceName}': {message}", innerException)
        {
            ResourceName = resourceName;
        }

        public string? ResourceName { get; }
    }

    public sealed class NotFoundException : BueiroInteligenteException
    {
        public NotFoundException(string message)
            : base(message) { }

        public NotFoundException(string resourceName, object? key)
            : base($"{resourceName} '{key}' was not found.")
        {
            ResourceName = resourceName;
            ResourceKey = key?.ToString();
        }

        public string? ResourceName { get; }

        public string? ResourceKey { get; }
    }
}
