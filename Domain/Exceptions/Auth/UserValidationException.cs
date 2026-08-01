namespace Domain.Exceptions.Auth;

public class UserValidationException : BadRequestException
{
    public UserValidationException()
        :base("Переданы невалидные данные пользователя")
    { }
}
