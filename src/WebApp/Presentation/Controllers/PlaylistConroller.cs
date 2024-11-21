using Application.Common.Helpers;
using Application.DTO.Playlists;
using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;

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
    public async Task<IActionResult> GetPlaylists()
    {
        return Ok(await _playlistService.GetPlaylistsAsync(HttpContext.RequestAborted));
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
        Result<PlaylistResponse> result = await _playlistService.GetPlaylistAsync(playlistId, User.GetUserId(), HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }


    [HttpPost]
    public async Task<IActionResult> CreatePlaylist()
    {
        var playlist = await _playlistService.CreatePlaylistAsync(User.GetUserId(), HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetPlaylist), new { playlistId = playlist.PlaylistId }, playlist);
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
        Result result = await _playlistService.DeletePlaylistAsync(playlistId, User.GetUserId()!);

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

    [HttpDelete]
    public async Task<IActionResult> DeletePlaylistPhoto(int playlistId)
    {
        Result result = await _playlistService.DeletePhotoAsync(playlistId);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPost("{PlaylistId}/addSong/{SongId}")]
    public async Task<IActionResult> AddSongToPlaylist(UpsertSongPlaylistRequest model, [FromServices] IValidator<UpsertSongPlaylistRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result result = await _playlistService.AddSongToPlaylistAsync(model, User.GetUserId()!, HttpContext.RequestAborted);

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

    [HttpDelete("{playlistId}/removeSong/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
    {
        Result result = await _playlistService.RemoveSongFromPlaylistAsync(songId, playlistId, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }
}
