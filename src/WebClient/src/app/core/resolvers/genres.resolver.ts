import { ResolveFn } from "@angular/router";
import { GenreListResponse } from "../models/genre/genreListResponse";
import { inject } from "@angular/core";
import { GenreService } from "../services/genre.service";

export const genresResolver: ResolveFn<GenreListResponse> = (route, state) => {
  const genreService = inject(GenreService);
  const genresQuery = { ...genreService.genresQuery };
  genresQuery.pageSize = 100;
  genresQuery.isSystemDefined = null;

  return inject(GenreService).getGenres(genresQuery);
}