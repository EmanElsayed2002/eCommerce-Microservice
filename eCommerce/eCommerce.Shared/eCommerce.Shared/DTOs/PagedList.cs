using System.Linq.Expressions;

namespace eCommerce.Shared.DTOs;

public class PagedList<T>(List<T> items, int count, int pageNumber, int pageSize)
{
    public List<T> Items => items;
    public int CurrentPage => pageNumber;
    public int TotalPages => (int)Math.Ceiling(count / (double)pageSize);
    public int PageSize => pageSize;
    public int TotalCount => count;
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public virtual PagedList<TTarget> MapTo<TTarget>(Func<T, TTarget> mapperFunc) => new(Items.Select(mapperFunc).ToList(), TotalCount, CurrentPage, PageSize);
}