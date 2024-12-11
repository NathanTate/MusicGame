using Application.Common.Helpers;
using Application.Models.Playlists;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;
using Application.Models.Queries;

namespace Presentation.Controllers;

[Route("playlists")]
[Authorize]
public class PlaylistConroller : BaseApiController
{
    private readonly IPlaylistService _playlistService;
    public PlaylistConroller(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlaylists([FromQuery] PlaylistsQueryRequest query)
    {
        return Ok(await _playlistService.GetPlaylistsAsync(query, HttpContext.RequestAborted));
    }

    [HttpGet("/api/{userId}/playlists")]
    public async Task<IActionResult> GetUserPlaylists(string userId)
    { 
        Result<List<PlaylistResponse>> result = null;
        return Ok();
    }

    [HttpGet("{playlistId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlaylist(int playlistId)
    {
        Result<PlaylistResponse> result = await _playlistService.GetPlaylistAsync(playlistId, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }


    [HttpPost]
    public async Task<IActionResult> CreatePlaylist()
    {
        Result<PlaylistResponse> result = await _playlistService.CreatePlaylistAsync(HttpContext.RequestAborted);

        return result.ToCreateHttpResponse(HttpContext);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePlaylist(UpdatePlaylistRequest model, [FromServices] IValidator<UpdatePlaylistRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<PlaylistResponse> result = await _playlistService.UpdatePlaylistAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{playlistId}")]
    public async Task<IActionResult> DeletePlaylist(int playlistId)
    {
        Result result = await _playlistService.DeletePlaylistAsync(playlistId);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPatch("{playlistId}/photo")]
    public async Task<IActionResult> UploadPlaylistPhoto(int playlistId, IFormFile photo)
    {
        string? errorMessage = PhotoFileValidator.Validate(photo);

        if (errorMessage is not null)
            return BadRequest(errorMessage);

        Result<PlaylistResponse> result = await _playlistService.UploadPhotoAsync(playlistId, photo, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{playlistId}/photo")]
    public async Task<IActionResult> DeletePlaylistPhoto(int playlistId)
    {
        Result result = await _playlistService.DeletePhotoAsync(playlistId);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("{PlaylistId}/songs/{SongId}")]
    public async Task<IActionResult> AddSongToPlaylist(UpsertSongPlaylistRequest model, [FromServices] IValidator<UpsertSongPlaylistRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _playlistService.AddSongToPlaylistAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPut("{PlaylistId}/updateSongPosition/{SongId}")]
    public async Task<IActionResult> UpdateSongPosition(UpsertSongPlaylistRequest model, [FromServices] IValidator<UpsertSongPlaylistRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);

        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _playlistService.UpdateSongPositionAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
    {
        Result result = await _playlistService.RemoveSongFromPlaylistAsync(songId, playlistId, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpGet("nameAvailable")]
    public async Task<IActionResult> IsPlaylistNameAvailable(string name)
    {
        return Ok(await _playlistService.IsPlaylistNameAvailable(name, HttpContext.RequestAborted));
    }
}
