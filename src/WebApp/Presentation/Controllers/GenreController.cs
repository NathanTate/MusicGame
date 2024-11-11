using Application.Common.Helpers;
using Application.DTO.Genres;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Presentation.Controllers;

[Route("genres")]
[Authorize]
public class GenreController : BaseApiController
{
    private readonly IGenreService _genreService;
    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] CreateGenreRequest model, [FromServices] IValidator<CreateGenreRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<GenreResponse> result = await _genreService.CreateGenreAsync(model, HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetGenre), new { genreId = result.Value.GenreId }, result.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenres()
    {
        List<GenreResponse> genres = await _genreService.GetGenresAsync(HttpContext.RequestAborted);

        return Ok(genres);
    }

    [HttpGet("{genreId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenre(int genreId)
    {
        Result<GenreResponse> result = await _genreService.GetGenreAsync(genreId, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateGenre([FromBody] UpdateGenreRequest model, [FromServices] IValidator<UpdateGenreRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<GenreResponse> result = await _genreService.UpdateGenreAsync(model, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpDelete("{genreId}")]
    public async Task<IActionResult> DeleteGenre(int genreId)
    {
        Result result = await _genreService.DeleteGenreAsync(genreId, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return NoContent();
    }
}
