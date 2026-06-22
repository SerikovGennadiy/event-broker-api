using Contracts.Repository;
using Entities.Domain.Contract;
using System.Linq.Expressions;

namespace Repository;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IdEntity
{
    protected AppDbContext _context;

    public RepositoryBase(AppDbContext context) => _context = context;

    public IQueryable<T> FindAll() => _context.Set<T>().AsQueryable();
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> condition) =>
        _context.Set<T>().Where(condition).AsQueryable();

    public void Create(T entity) => _context.Set<T>().Add(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
}
