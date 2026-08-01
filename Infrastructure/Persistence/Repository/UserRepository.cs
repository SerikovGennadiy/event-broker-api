using Application.Contracts.Persistance;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {  }

    public void CreateUser(User user) => Create(user);
    public async Task<ICollection<User>> GetAllUsers() => await FindAll().ToListAsync();
    public async Task<User?> GetUserByIdAsync(Guid userId) => await FindByCondition(user => user.Id == userId).FirstOrDefaultAsync();
    public async Task<User?> GetUserByNameAsync(string userName) => await FindByCondition(user => user.Name == userName).FirstOrDefaultAsync();
}
