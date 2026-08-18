namespace Domain.Exceptions.Auth;

public class AccessDeniedException : UnauthorizedException
{
    public AccessDeniedException(string message):
        base($"Операция вам запрещена: {message}") { }
}
