using Domain.Contracts;

namespace Domain.Models;

/// <summary>Сущность пользователя</summary>
public class User : IdEntity
{
    /// <summary>Идентификатор пользователя</summary>
    public Guid Id { get; init; }

    /// <summary>Логин пользователя</summary>
    public required string Login { get; init; }

    /// <summary>Хеш пароля</summary>
    public required string PasswordHash { get; init; }

    /// <summary>Роль пользователя</summary>
    public Role Role { get; private set; }

    // приватный конструктор для EFCore
    private User() { }

    public static User Create(string login, string passwordHash, Role role = Role.User)
    {
       return new User()
        {
            Id = Guid.CreateVersion7(),
            Login = login,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}

public enum Role
{
    User,
    Admin
}