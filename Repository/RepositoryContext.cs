using Entities.Models;

namespace Repository
{
    public class RepositoryContext
    {
        private readonly Dictionary<Type, object> _dbSets = new();
        public List<Event> Events { get; set; } = [];

        public RepositoryContext()
        {
            // получить все List'ы в классе
            var dbSets = GetType().GetProperties()
                                  .Where(p => p.PropertyType.IsGenericType 
                                            && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                                  .ToList();

            // зарегитрировать их, чтобы получать доступ к ним по типу
            dbSets.ForEach(prop =>
            {
                var dbSetType = prop.PropertyType.GetGenericArguments()[0];
                _dbSets[dbSetType] = prop.GetValue(this);
            });
        }

        public List<T> Set<T>()
        {
            if(_dbSets.TryGetValue(typeof(T), out var dbSet))
            {
                return (List<T>)dbSet;
            }
            throw new ArgumentException($"В хранилище данных не зарегистрирована модель {nameof(T)}");
        }
    }
}
