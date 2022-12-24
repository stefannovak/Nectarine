namespace NectarineAPI.Models;

public class Pagination
{
    public Pagination(bool hasMore, int pageNumber, int pageSize, int? totalCount)
    {
        HasMore = hasMore;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public bool HasMore { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int? TotalCount { get; set; }
}