namespace ItemHub.Models;

public class PageViewModel
{
    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }
 
    public PageViewModel(int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
 
    public bool HasPreviousPage(int page) => (PageNumber > page);
    public bool HasNextPage(int page) => (PageNumber + page < TotalPages);
}