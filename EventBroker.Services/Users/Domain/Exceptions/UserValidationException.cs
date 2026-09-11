using Users.Domain.Exceptions.Common;

namespace Users.Domain.Exceptions;

public class UserValidationException : BadRequestException
{
    public UserValidationException()
        : base("Переданы невалидные данные пользователя")
    { }
}
