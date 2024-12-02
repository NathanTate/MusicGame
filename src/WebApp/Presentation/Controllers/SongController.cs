using Application.Common.Helpers;
using Application.DTO.Songs;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Route("api/songs")]
[Authorize]
public class SongController : BaseApiController
{
    private readonly ISongService _songService;
    public SongController(ISongService songService)
    {
        _songService = songService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSong(CreateSongRequest model, [FromServices] IValidator<CreateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<SongResponse> result = await _songService.CreateSongAsync(model, User.GetUserId()!, HttpContext.RequestAborted);

        return result.ToCreateHttpResponse(HttpContext);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSongs()
    {
        List<SongResponse> songs = await _songService.GetSongsAsync(HttpContext.RequestAborted);

        return Ok(songs);
    }

    [HttpGet("{songId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSong(int songId)
    {
        Result<SongResponse> result = await _songService.GetSongAsync(songId, User.GetUserId(), HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSong([FromBody] UpdateSongRequest model, [FromServices] IValidator<UpdateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<SongResponse> result = await _songService.UpdateSongAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{songId}")]
    public async Task<IActionResult> DeleteSong(int songId)
    {
        Result result = await _songService.DeleteSongAsync(songId, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPatch("{songId}/photo")]
    public async Task<IActionResult> UploadSongPhoto(int songId, IFormFile photo)
    {
        string? erroMessage = PhotoFileValidator.Validate(photo);

        if (erroMessage is not null)
            return BadRequest(erroMessage);

        Result<SongResponse> result = await _songService.UploadPhotoAsync(songId, photo);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{songId}/photo")]
    public async Task<IActionResult> DeleteSongPhoto(int songId)
    {
        Result result = await _songService.DeletePhotoAsync(songId);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpGet("nameAvailable")]
    public async Task<IActionResult> IsSongNameAvailable(string name)
    {
        return Ok(await _songService.IsSongNameAvailableAsync(name, HttpContext.RequestAborted));
    }
}
