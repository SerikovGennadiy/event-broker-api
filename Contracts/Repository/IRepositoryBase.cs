namespace Contracts.Repository;

public interface IRepositoryBase<T>
{
    IQueryable<T> FindAll();
    IQueryable<T> FindByCondition(Func<T, bool> condition);

    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
