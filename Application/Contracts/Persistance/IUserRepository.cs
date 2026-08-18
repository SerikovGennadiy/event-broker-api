using Domain.Models;

namespace Application.Contracts.Persistance;

public interface IUserRepository
{
    Task<ICollection<User>> GetAllUsers();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByNameAsync(string userName);
    void CreateUser(User user);
}
