namespace Application.Models.Queries;
public class GenresQueryRequest : BaseQuery
{
    public string? SearchTerm { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
    public bool? IsSystemDefined { get; set; } = null;
}
