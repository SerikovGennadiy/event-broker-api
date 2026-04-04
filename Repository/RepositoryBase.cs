using Contracts.Repository;
using Entities.Contract;

namespace Repository;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IdEntity
{
    protected RepositoryContext _context;

    public RepositoryBase(RepositoryContext context) => _context = context;

    public IEnumerable<T> FindAll() => _context.Set<T>().AsEnumerable();
    public IEnumerable<T> FindByCondition(Func<T, bool> condition) =>
        _context.Set<T>().Where(condition).AsEnumerable();

    public void Create(T entity) => _context.Set<T>().Add(entity);
    public void Delete(T entity) => _context.Set<T>().Remove(entity);
    public void Update(T entity) => _context.Set<T>().Update(entity);
}
