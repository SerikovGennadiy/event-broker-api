using Users.Application.Contracts.Persistence;

namespace Users.Infrastructure.Persistance.Repository;

public class RepositoryManager(AppDbContext context) : IRepositoryManager
{
    private IUserRepository Users { get; } = new UserRepository(context);
    public IUserRepository User => Users;

    public async Task SaveAsync() => await context.SaveChangesAsync();
}
