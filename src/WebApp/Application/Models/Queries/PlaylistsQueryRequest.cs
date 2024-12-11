namespace Application.Models.Queries;
public class PlaylistsQueryRequest : BaseQuery
{
    public string SearchTerm { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
    public string SortColumn { get; set; } = "";
    public bool? IsPrivate { get; set; } = null;
}
