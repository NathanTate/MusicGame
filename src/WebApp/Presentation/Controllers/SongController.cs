using Application.Common.Helpers;
using Application.DTO.Songs;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace Presentation.Controllers;

[Route("songs")]
[Authorize]
public class SongController : BaseApiController
{
    private readonly ISongService _songService;
    public SongController(ISongService songService)
    {
        _songService = songService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(CreateSongRequest model, [FromServices] IValidator<CreateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        string userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        Result<SongResponse> result = await _songService.CreateSongAsync(model, userId, HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetGenres()
    {
        List<SongResponse> genres = await _songService.GetSongsAsync(HttpContext.RequestAborted);

        return Ok(genres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenre(int id)
    {
        Result<SongResponse> result = await _songService.GetSongAsync(id, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateGenre([FromBody] UpdateSongRequest model, [FromServices] IValidator<UpdateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);


        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        Result result = await _songService.DeleteSongAsync(id, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok();
    }
}
