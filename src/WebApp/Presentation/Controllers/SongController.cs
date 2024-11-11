using Application.Common.Helpers;
using Application.DTO.Songs;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;
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
    public async Task<IActionResult> CreateSong(CreateSongRequest model, [FromServices] IValidator<CreateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<SongResponse> result = await _songService.CreateSongAsync(model, User.GetUserId(), HttpContext.RequestAborted);

        if (result.IsFailed)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetSong), new { songId = result.Value.SongId}, result.Value);
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
        Result<SongResponse> result = await _songService.GetSongAsync(songId, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSong([FromBody] UpdateSongRequest model, [FromServices] IValidator<UpdateSongRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<SongResponse> result = await _songService.UpdateSongAsync(model, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpDelete("{songId}")]
    public async Task<IActionResult> DeleteSong(int songId)
    {
        Result result = await _songService.DeleteSongAsync(songId, HttpContext.RequestAborted);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return NoContent();
    }

    [HttpPatch("{songId}/photo")]
    public async Task<IActionResult> UploadSongPhoto(int songId, IFormFile photo)
    {
        string? erroMessage = PhotoFileValidator.Validate(photo);

        if (erroMessage is not null)
            return BadRequest(erroMessage);

        Result<SongResponse> result = await _songService.UploadPhotoAsync(songId, photo);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpDelete("{songId}/photo")]
    public async Task<IActionResult> DeleteSongPhoto(int songId)
    {
        Result<SongResponse> result = await _songService.DeletePhotoAsync(songId);

        if (result.IsFailed)
            return NotFound(result.Errors);

        return NoContent();
    }
}
