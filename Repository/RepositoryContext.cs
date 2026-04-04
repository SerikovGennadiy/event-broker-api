using Entities.Domain.Contract;
using Entities.Domain.Models;

namespace Repository;

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

public static class ListUpdateExtension
{
    public static List<T> Update<T>(this List<T> list, T item) where T : class, IdEntity
    {
        var stored = list.FirstOrDefault(x => x.Id == item.Id);
        if (stored == null)
            throw new ArgumentException($"Сущность {nameof(T)} с ID: {item.Id} отсутсвует");

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead 
                     && p.CanWrite 
                     && p.Name != $"{nameof(item.Id)}");

        foreach(var prop in properties)
        {
            var value = prop.GetValue(item);
            prop.SetValue(stored, value);
        }

        return list;
    }
}
