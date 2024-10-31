using Application.InfrastructureInterfaces;
using Application.Interfaces;

namespace Application.Services;
internal class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }
}
