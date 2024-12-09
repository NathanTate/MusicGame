﻿using Application.Common.Helpers;
using Application.DTO.Genres;
using Application.Interfaces;
using Domain.Enums;
using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Presentation.Extensions;

namespace Presentation.Controllers;

[Route("api/genres")]
[Authorize(Roles = nameof(Role.ADMIN))]
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

        return result.ToCreateHttpResponse(HttpContext);
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

        return result.ToHttpResponse(HttpContext);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateGenre([FromBody] UpdateGenreRequest model, [FromServices] IValidator<UpdateGenreRequest> validator)
    {
        ModelStateDictionary errors = await Validator.ValidateAsync(validator, model, HttpContext.RequestAborted);
        if (errors.Count > 0)
            return ValidationProblem(errors);

        Result<GenreResponse> result = await _genreService.UpdateGenreAsync(model, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }

    [HttpDelete("{genreId}")]
    public async Task<IActionResult> DeleteGenre(int genreId)
    {
        Result result = await _genreService.DeleteGenreAsync(genreId, HttpContext.RequestAborted);

        return result.ToHttpResponse(HttpContext);
    }
}
