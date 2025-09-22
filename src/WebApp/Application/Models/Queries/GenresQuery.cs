namespace Application.Models.Queries;
public class GenresQuery : BaseQuery
{
    public string? SearchTerm { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
    public bool? IsSystemDefined { get; set; } = null;
}
