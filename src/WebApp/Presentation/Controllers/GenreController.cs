using Application.Interfaces;

namespace Presentation.Controllers;

public class GenreController : BaseApiController
{
    private readonly IGenreService _genreService;
    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }
}
