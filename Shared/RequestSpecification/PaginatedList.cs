namespace Shared.RequestSpecification;

public class PaginatedList<T> : List<T>
{
    public PaginatedResult PageMetaData { get; set; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageMetaData = new PaginatedResult
        {
            CurrentPageNumber = pageNumber,
            EntitiesCountPerPage = pageSize,
            EntitiesCountTotal = count,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize)
        };

        AddRange(items);
    }

    public static PaginatedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize).ToList();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
