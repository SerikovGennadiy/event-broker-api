using System.Linq.Expressions;

namespace Application.Contracts.Persistance;

public interface IRepositoryBase<T>
{
    IQueryable<T> FindAll();
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> condition);

    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
