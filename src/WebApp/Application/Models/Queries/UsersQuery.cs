namespace Application.Models.Queries;
public class UsersQuery : BaseQuery
{
    public string? SearchTerm { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
    public string? SortColumn { get; set; } = "";
}
