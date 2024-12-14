namespace ItemHub.Models.Pages;

public class PageViewModel(int count, int pageNumber, int pageSize)
{
    public int PageNumber { get; private set; } = pageNumber;
    public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);

    public bool HasPreviousPage(int page) => (PageNumber > page);
    public bool HasNextPage(int page) => (PageNumber + page < TotalPages);
}