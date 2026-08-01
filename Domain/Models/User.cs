using Domain.Contracts;

namespace Domain.Models;

/// <summary>Сущность пользователя</summary>
public class User : IdEntity
{
    /// <summary>Идентификатор пользователя</summary>
    public Guid Id { get; init; }

    /// <summary>Логин пользователя</summary>
    public required string Name { get; init; }

    /// <summary>Хеш пароля</summary>
    public string? PasswordHash { get; init; }

    /// <summary>Роль пользователя</summary>
    public Role Role { get; private set; }

    // приватный конструктор для EFCore
    private User() { }

    public static User Restore(Guid guid, string userName, string passwordHash, Role role = Role.User)
    {
        return new User()
        {
            Id = guid,
            Name = userName,
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public static User Empty() => new User() { Id = Guid.Empty, Name = string.Empty, PasswordHash = string.Empty, Role = Role.User };
}

public enum Role
{
    User = 0,
    Admin = 1
}