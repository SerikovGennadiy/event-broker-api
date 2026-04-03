using Contracts;
using Entities.Contract;

namespace Repository
{
    internal class RepositoryBase<T> : IRepositoryBase<T> where T : class, IdEntity
    {
        protected RepositoryContext _context;

        public RepositoryBase(RepositoryContext context) => _context = context;

        public IEnumerable<T> FindAll() => _context.Set<T>().AsEnumerable();
        public IEnumerable<T> FindByCondition(Func<T, bool> condition) =>
            _context.Set<T>().Where(condition).AsEnumerable();

        public void Create(T entity) => _context.Set<T>().Add(entity);
        public void Delete(T entity) => _context.Set<T>().Remove(entity);
        public void Update(T entity)
        {
            var stored = _context.Set<T>().Find(x => x.Id == entity.Id);
            if (stored is null)
                throw new InvalidOperationException($"Сущность с ID {entity.Id} не найдена.");

            var properties = typeof(T).GetProperties()
                .Where(prop => prop.CanRead && prop.CanWrite && prop.Name != "Id");

            foreach(var prop in properties)
            {
                var value = prop.GetValue(entity);
                prop.SetValue(stored, value);
            }
        }
    }
}
