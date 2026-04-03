namespace Contracts;

internal interface IRepositoryBase<T>
{
    IEnumerable<T> FindAll();
    IEnumerable<T> FindByCondition(Func<T, bool> condition);

    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
