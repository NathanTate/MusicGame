namespace Application.Models.Queries;
public class ElasticQuery
{
    private const int _MaxPageSize = 50;
    private int _pageSize;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < _MaxPageSize ? value : _MaxPageSize;
    }

    public int Page { get; set; }
    public string? SearchTerm { get; set; }
}
