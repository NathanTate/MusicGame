namespace Application.Models.Queries;
public class BaseQuery
{
    private const int _MaxPageSize = 50;
    private int _pageSize = 20;
    public int PageSize 
    { 
        get => _pageSize;
        set => _pageSize = value < _MaxPageSize ? value : _MaxPageSize; 
    }
    public int Page { get; set; } = 1;
}
