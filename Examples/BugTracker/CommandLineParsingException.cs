// See https://aka.ms/new-console-template for more information
using System.Runtime.Serialization;

[Serializable]
internal class CommandLineParsingException : Exception
{
    public CommandLineParsingException()
    {
    }

    public CommandLineParsingException(string? message) : base(message)
    {
    }

    public CommandLineParsingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CommandLineParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}