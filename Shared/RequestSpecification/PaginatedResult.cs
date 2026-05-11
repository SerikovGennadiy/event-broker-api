namespace Shared.RequestSpecification;

public class PaginatedResult
{
    public int CurrentPageNumber { get; set; }
    public int EntitiesCountPerPage { get; set; }
    public int TotalPages { get; set; }
    public int EntitiesCountTotal { get; set; }

    public bool HasPrevious => CurrentPageNumber > 1;
    public bool HasNext => CurrentPageNumber < TotalPages;
}
