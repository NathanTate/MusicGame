import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.development";
import { CreateGenreRequest } from "../models/genre/createGenreRequest";
import { HttpClient } from "@angular/common/http";
import { GenreResponse } from "../models/genre/genreResponse";
import { GenresQuery } from "../models/queries/genresQuery";
import { GenreListResponse } from "../models/genre/genreListResponse";
import { generateHttpParams } from "../../shared/helpers/httpParamsHelper";
import { UpdateGenreRequest } from "../models/genre/updateGenreRequest";

@Injectable({
  providedIn: 'root'
})
export class GenreService {
  private readonly _baseUrl = environment.apiUrl + 'genres/';
  private readonly http = inject(HttpClient);

  createGenre(model: CreateGenreRequest) {
    return this.http.post<GenreResponse>(this._baseUrl, model);
  }

  getGenres(query: GenresQuery) {
    const httpParams = generateHttpParams(query);

    return this.http.get<GenreListResponse>(this._baseUrl, { params: httpParams });
  }

  getGenre(genreId: number) {
    return this.http.get<GenreResponse>(this._baseUrl + genreId);
  }

  updateGenre(model: UpdateGenreRequest) {
    return this.http.put<GenreResponse>(this._baseUrl, model);
  }

  deleteGenre(genreId: number) {
    return this.http.delete<void>(this._baseUrl + genreId);
  }

  isNameAvailable(name: string) {
    return this.http.post<boolean>(this._baseUrl + 'nameAvailable', { name });
  }
}