namespace Shared;

public class PageResult<T>
{
    public List<T> items { get; set; } = new();
    public int totalCount { get; set; }
    public int page { get; set; }
    public int pageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)totalCount / pageSize);
    public bool HasPreviousPage => page > 1;
    public bool HasNextPage => page < TotalPages;
}