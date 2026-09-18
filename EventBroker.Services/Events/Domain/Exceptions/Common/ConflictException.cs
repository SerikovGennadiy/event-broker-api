namespace Events.Domain.Exceptions.Common;

public abstract class ConflictException : Exception
{
    protected ConflictException(string? message) : base(message)
    { }
}
